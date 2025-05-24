using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class DatabaseWriter
    {
        private SqliteConnection db;
        private SqliteTransaction _transaction;
        
        private SqliteCommand _addGameAssetCommand;
        private SqliteCommand _addModAssetCommand;
        private SqliteCommand _addPropertyDataCommand;
        private SqliteCommand _addScriptTypeCommand;
        private SqliteCommand _addComponentTypeCommand;
        private SqliteCommand _addAssetSourceCommand;

        private Dictionary<string, long> _componentTypeCache;
        private Dictionary<string, long> _scriptTypeCache;

        public DatabaseWriter(string databasePath)
        {
            try
            {
                db = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Pooling = false,
                    ForeignKeys = false
                }.ConnectionString);
                File.WriteAllBytes(databasePath, Array.Empty<byte>());
                db.Open();
                
                var command = db.CreateCommand();
                command.CommandText = Resources.Init;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR OPENING ASSET REFERENCE DATABASE at {databasePath}");
                throw;
            }
            
            _componentTypeCache = new Dictionary<string, long>();
            _scriptTypeCache = new Dictionary<string, long>();
            
            CreateSQLCommands();
        }

        public long InsertPropertyData(string componentType, string propertyName, bool isCollection = false, long collectionIndex = -1, string scriptName = "None")
        {
            try
            {
                _transaction = db.BeginTransaction();
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

        public void InsertGameAsset(string sceneName, string assetGuid, string baseGameObject, string transformPath, long propertyDataId)
        {
            try
            {
                _transaction = db.BeginTransaction();
                _addGameAssetCommand.Transaction = _transaction;
                _addGameAssetCommand.Parameters["@guid"].Value = assetGuid;
                _addGameAssetCommand.Parameters["@name"].Value = sceneName;
                _addGameAssetCommand.Parameters["@source_id"].Value =
                    InsertAssetSource(baseGameObject, transformPath, propertyDataId);
                _addGameAssetCommand.ExecuteNonQuery();

                _transaction.Commit();
            }
            catch (Exception e)
            {
                _transaction.Rollback();
                throw;
            }
        }

        public void InsertModAsset(string modGuid, string assetPath, string assetGuid, string baseGameObject, string transformPath, long propertyDataId)
        {
            try
            {
                _transaction = db.BeginTransaction();
                _addModAssetCommand.Transaction = _transaction;
                _addModAssetCommand.Parameters["@guid"].Value = assetGuid;
                _addModAssetCommand.Parameters["@mod_guid"].Value = modGuid;
                _addModAssetCommand.Parameters["@name"].Value = assetPath;
                _addModAssetCommand.Parameters["@source_id"].Value =
                    InsertAssetSource(baseGameObject, transformPath, propertyDataId);
                _addModAssetCommand.ExecuteNonQuery();

                _transaction.Commit();
            }  catch (Exception e)
            {
                _transaction.Rollback();
                throw;
            }
        }

        private long InsertAssetSource(string baseGameObject, string transformPath, long propertyDataId)
        {
            _addAssetSourceCommand.Transaction = _transaction;
            _addAssetSourceCommand.Parameters["@gameobject"].Value = baseGameObject;
            _addAssetSourceCommand.Parameters["@path"].Value = transformPath;
            _addAssetSourceCommand.Parameters["@prop_id"].Value = propertyDataId;
            return (long)_addAssetSourceCommand.ExecuteScalar();
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
            _addGameAssetCommand = db.CreateCommand();
            _addGameAssetCommand.CommandText = "INSERT INTO game_assets (asset_guid, asset_name, source_id) VALUES (@guid, @name, @source_id)";
            _addGameAssetCommand.Parameters.Add("@guid", SqliteType.Text);
            _addGameAssetCommand.Parameters.Add("@name", SqliteType.Text);
            _addGameAssetCommand.Parameters.Add("@source_id", SqliteType.Integer);

            _addModAssetCommand = db.CreateCommand();
            _addModAssetCommand.CommandText = "INSERT INTO mod_assets (mod_guid, asset_guid, asset_name, source_id) VALUES (@mod_guid, @guid, @name, @source_id)";
            _addModAssetCommand.Parameters.Add("@mod_guid", SqliteType.Text);
            _addModAssetCommand.Parameters.Add("@guid", SqliteType.Text);
            _addModAssetCommand.Parameters.Add("@name", SqliteType.Text);
            _addModAssetCommand.Parameters.Add("@source_id", SqliteType.Integer);

            _addComponentTypeCommand = db.CreateCommand();
            _addComponentTypeCommand.CommandText = "INSERT OR IGNORE INTO component_types (type) VALUES(@type);SELECT id FROM component_types WHERE type = @type;";
            _addComponentTypeCommand.Parameters.Add("@type", SqliteType.Text);
            
            _addScriptTypeCommand = db.CreateCommand();
            _addScriptTypeCommand.CommandText = "INSERT OR IGNORE INTO script_types (type) VALUES(@type);SELECT id FROM script_types WHERE type = @type;";
            _addScriptTypeCommand.Parameters.Add("@type", SqliteType.Text);

            _addPropertyDataCommand = db.CreateCommand();
            _addPropertyDataCommand.CommandText =
                "INSERT INTO property_data (component_id, script_id, property_name, is_collection, collection_index) VALUES (@component, @script, @property, @collection, @collection_idx); SELECT last_insert_rowid();";
            _addPropertyDataCommand.Parameters.Add("@component", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@script", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@property", SqliteType.Text);
            _addPropertyDataCommand.Parameters.Add("@collection", SqliteType.Integer);
            _addPropertyDataCommand.Parameters.Add("@collection_idx", SqliteType.Integer);

            _addAssetSourceCommand = db.CreateCommand();
            _addAssetSourceCommand.CommandText =
                "INSERT INTO asset_sources (base_gameobject, transform_path, property_id) VALUES (@gameobject, @path, @prop_id); SELECT last_insert_rowid();";
            _addAssetSourceCommand.Parameters.Add("@gameobject", SqliteType.Text);
            _addAssetSourceCommand.Parameters.Add("@path", SqliteType.Text);
            _addAssetSourceCommand.Parameters.Add("@prop_id", SqliteType.Integer);
        }
        
        
    }
}