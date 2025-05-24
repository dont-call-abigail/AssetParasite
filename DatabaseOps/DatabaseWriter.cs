using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class DatabaseWriter : DatabaseInterface
    {
        private SqliteTransaction _transaction;
        
        private SqliteCommand _addModAssetCommand;
        private SqliteCommand _addPropertyDataCommand;
        private SqliteCommand _addScriptTypeCommand;
        private SqliteCommand _addComponentTypeCommand;
        private SqliteCommand _addAssetLocationCommand;

        private Dictionary<string, long> _componentTypeCache;
        private Dictionary<string, long> _scriptTypeCache;

        public DatabaseWriter(string databasePath) : base(databasePath)
        {
            var command = _db.CreateCommand();
            command.CommandText = Resources.Init;
            command.ExecuteNonQuery();

            _componentTypeCache = new Dictionary<string, long>();
            _scriptTypeCache = new Dictionary<string, long>();
            
            CreateSQLCommands();
        }

        public long InsertPropertyData(string componentType, string propertyName, bool isCollection = false, long collectionIndex = -1, string scriptName = "None")
        {
            try
            {
                _transaction = _db.BeginTransaction();
                _addPropertyDataCommand.Transaction = _transaction;
                _addPropertyDataCommand.Parameters["@component"].Value = GetComponentTypeId(componentType);
                _addPropertyDataCommand.Parameters["@script"].Value = GetScriptTypeId(scriptName);
                _addPropertyDataCommand.Parameters["@property"].Value = propertyName;
                _addPropertyDataCommand.Parameters["@collection"].Value = isCollection ? 1 : 0;
                _addPropertyDataCommand.Parameters["@collection_idx"].Value = collectionIndex;

                var res = (long)_addPropertyDataCommand.ExecuteScalar();
                _transaction.Commit();
                return res;
            }
            catch (Exception e)
            {
                _transaction.Rollback();
                throw;
            }
        }

        public void InsertAsset(string modGuid, string assetPath, string assetGuid, string baseGameObject, string transformPath, long propertyDataId)
        {
            try
            {
                _transaction = _db.BeginTransaction();
                _addModAssetCommand.Transaction = _transaction;
                _addModAssetCommand.Parameters["@guid"].Value = assetGuid;
                _addModAssetCommand.Parameters["@source"].Value = modGuid;
                _addModAssetCommand.Parameters["@name"].Value = assetPath;
                _addModAssetCommand.Parameters["@location_id"].Value =
                    InsertAssetLocation(baseGameObject, transformPath, propertyDataId);
                _addModAssetCommand.ExecuteNonQuery();

                _transaction.Commit();
            }  catch (Exception e)
            {
                _transaction.Rollback();
                throw;
            }
        }

        private long InsertAssetLocation(string baseGameObject, string transformPath, long propertyDataId)
        {
            _addAssetLocationCommand.Transaction = _transaction;
            _addAssetLocationCommand.Parameters["@gameobject"].Value = baseGameObject;
            _addAssetLocationCommand.Parameters["@path"].Value = transformPath;
            _addAssetLocationCommand.Parameters["@prop_id"].Value = propertyDataId;
            return (long)_addAssetLocationCommand.ExecuteScalar();
        }

        private long GetComponentTypeId(string type)
        {
            if (_componentTypeCache.TryGetValue(type, out long id))
            {
                return id;
            }
            else
            {
                _addComponentTypeCommand.Transaction = _transaction;
                _addComponentTypeCommand.Parameters["@type"].Value = type;
                var res = (long?)_addComponentTypeCommand.ExecuteScalar();
                if (res.HasValue) _componentTypeCache.Add(type, res.Value);
            }

            return -1;
        }

        private long GetScriptTypeId(string type)
        {
            if (_scriptTypeCache.TryGetValue(type, out long id))
            {
                return id;
            }
            else
            {
                _addScriptTypeCommand.Transaction = _transaction;
                _addScriptTypeCommand.Parameters["@type"].Value = type;
                var res = (long?)_addScriptTypeCommand.ExecuteScalar();
                if (res.HasValue) _scriptTypeCache.Add(type, res.Value);
            }

            return -1;
        }
        
        private void CreateSQLCommands()
        {
            _addModAssetCommand = _db.CreateCommand();
            _addModAssetCommand.CommandText = "INSERT INTO assets (source, asset_guid, asset_name, location_id) VALUES (@source, @guid, @name, @location_id)";
            _addModAssetCommand.Parameters.Add("@source", SqliteType.Text);
            _addModAssetCommand.Parameters.Add("@guid", SqliteType.Text);
            _addModAssetCommand.Parameters.Add("@name", SqliteType.Text);
            _addModAssetCommand.Parameters.Add("@location_id", SqliteType.Integer);

            _addComponentTypeCommand = _db.CreateCommand();
            _addComponentTypeCommand.CommandText = "INSERT OR IGNORE INTO component_types (type) VALUES(@type);SELECT id FROM component_types WHERE type = @type;";
            _addComponentTypeCommand.Parameters.Add("@type", SqliteType.Text);
            
            _addScriptTypeCommand = _db.CreateCommand();
            _addScriptTypeCommand.CommandText = "INSERT OR IGNORE INTO script_types (type) VALUES(@type);SELECT id FROM script_types WHERE type = @type;";
            _addScriptTypeCommand.Parameters.Add("@type", SqliteType.Text);

            _addPropertyDataCommand = _db.CreateCommand();
            _addPropertyDataCommand.CommandText =
                "INSERT INTO property_data (component_id, script_id, property_name, is_collection, collection_index) VALUES (@component, @script, @property, @collection, @collection_idx); SELECT last_insert_rowid();";
            _addPropertyDataCommand.Parameters.Add("@component", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@script", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@property", SqliteType.Text);
            _addPropertyDataCommand.Parameters.Add("@collection", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@collection_idx", SqliteType.Integer);

            _addAssetLocationCommand = _db.CreateCommand();
            _addAssetLocationCommand.CommandText =
                "INSERT INTO asset_locations (base_gameobject, transform_path, property_id) VALUES (@gameobject, @path, @prop_id); SELECT last_insert_rowid();";
            _addAssetLocationCommand.Parameters.Add("@gameobject", SqliteType.Text);
            _addAssetLocationCommand.Parameters.Add("@path", SqliteType.Text);
            _addAssetLocationCommand.Parameters.Add("@prop_id", SqliteType.Integer);
        }
        
        
    }
}