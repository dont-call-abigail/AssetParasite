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
        private SqliteCommand _addComponentTypeCommand;
        private SqliteCommand _addAssetLocationCommand;
        private SqliteCommand _removeModAssetsCommand;

        private Dictionary<string, string> _componentIdCache;

        public DatabaseWriter(string databasePath) : base(databasePath)
        {
            CreateSQLCommands();
            _componentIdCache = new Dictionary<string, string>();
        }

        public long InsertPropertyData(string componentType, string propertyName, bool isCollection = false, long collectionIndex = -1, string componentId = null)
        {
            string trueComponentId;
            if (_componentIdCache.TryGetValue(componentType, out string id))
            {
                trueComponentId = id;
            }
            else
            {
                trueComponentId = InsertComponentType(componentType, componentId ?? Guid.NewGuid().ToString("N"));
                _componentIdCache.Add(componentType, trueComponentId);
            }
            
            try
            {
                _transaction = _db.BeginTransaction();
                _addPropertyDataCommand.Transaction = _transaction;
                _addPropertyDataCommand.Parameters["@component"].Value = trueComponentId;
                _addPropertyDataCommand.Parameters["@property"].Value = propertyName;
                _addPropertyDataCommand.Parameters["@collection"].Value = isCollection ? 1 : 0;
                _addPropertyDataCommand.Parameters["@collection_idx"].Value = collectionIndex;

                var res = (long)_addPropertyDataCommand.ExecuteScalar();
                _transaction.Commit();
                return res;
            }
            catch (Exception)
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
            }  catch (Exception)
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

        public string InsertComponentType(string typeName, string guid)
        {
            _transaction = _db.BeginTransaction();
            _addComponentTypeCommand.Transaction = _transaction;
            _addComponentTypeCommand.Parameters["@id"].Value = guid;
            _addComponentTypeCommand.Parameters["@type"].Value = typeName;
            string res = (string)_addComponentTypeCommand.ExecuteScalar();
            _transaction.Commit();
            return res; 
        }

        public bool RemoveModAssets(string modGuid)
        {
            _transaction = _db.BeginTransaction();
            _removeModAssetsCommand.Transaction = _transaction;
            _removeModAssetsCommand.Parameters["@source"].Value = modGuid;
            int res = _removeModAssetsCommand.ExecuteNonQuery();
            _transaction.Commit();
            return res > 0;
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
            _addComponentTypeCommand.CommandText = "INSERT OR IGNORE INTO component_types (id, type) VALUES(@id, @type); SELECT id FROM component_types WHERE type = @type; ";
            _addComponentTypeCommand.Parameters.Add("@id", SqliteType.Text);
            _addComponentTypeCommand.Parameters.Add("@type", SqliteType.Text);

            _addPropertyDataCommand = _db.CreateCommand();
            _addPropertyDataCommand.CommandText =
                "INSERT INTO property_data (component_id, property_name, is_collection, collection_index) VALUES (@component, @property, @collection, @collection_idx); SELECT last_insert_rowid();";
            _addPropertyDataCommand.Parameters.Add("@component", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@property", SqliteType.Text);
            _addPropertyDataCommand.Parameters.Add("@collection", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@collection_idx", SqliteType.Integer);

            _addAssetLocationCommand = _db.CreateCommand();
            _addAssetLocationCommand.CommandText =
                "INSERT INTO asset_locations (base_gameobject, transform_path, property_id) VALUES (@gameobject, @path, @prop_id); SELECT last_insert_rowid();";
            _addAssetLocationCommand.Parameters.Add("@gameobject", SqliteType.Text);
            _addAssetLocationCommand.Parameters.Add("@path", SqliteType.Text);
            _addAssetLocationCommand.Parameters.Add("@prop_id", SqliteType.Integer);

            _removeModAssetsCommand = _db.CreateCommand();
            _removeModAssetsCommand.CommandText = "DELETE FROM assets WHERE source = @source";
            _removeModAssetsCommand.Parameters.Add("@source", SqliteType.Text);
        }
        
        
    }
}