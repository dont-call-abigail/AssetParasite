using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class DatabaseReader : DatabaseInterface
    {
        private SqliteTransaction _transaction;
        
        private SqliteCommand _getComponentTypeCommand;
        private SqliteCommand _entryExistsCommand;
        private SqliteCommand _getEntriesCommand;
        private SqliteCommand _getMatchingAssetsCommand;

        public class AssetLookupResult
        {
            public string Source;
            public string Guid;
            public string BaseGameObject;
            public int[] TransformPath;
            public string Property;
            public bool IsCollection;
            public int CollectionIndex;
            public string ComponentType;
        }

        public DatabaseReader(string databasePath) : base(databasePath)
        {
            CreateSQLCommands();
        }

        public string GetComponentType(string scriptGuid)
        {
            _transaction = _db.BeginTransaction();
            _getComponentTypeCommand.Transaction = _transaction;
            _getComponentTypeCommand.Parameters["@id"].Value = scriptGuid;
            var res = Convert.ToString(_getComponentTypeCommand.ExecuteScalar());
            _transaction.Commit();
            return !string.IsNullOrEmpty(res) ? res : "MonoBehaviour";
        }

        public bool EntryExistsForAssetId(string assetGuid, string source)
        {
            _transaction = _db.BeginTransaction();
            _entryExistsCommand.Transaction = _transaction;
            _entryExistsCommand.Parameters["@asset_guid"].Value = assetGuid;
            _entryExistsCommand.Parameters["@source"].Value = source;
            var res = Convert.ToInt32(_entryExistsCommand.ExecuteScalar());
            _transaction.Commit();
            return res > 0;
        }

        public string[] GetIdsForAsset(string assetName, string source)
        {
            _transaction = _db.BeginTransaction();
            _getEntriesCommand.Transaction = _transaction;
            _getEntriesCommand.Parameters["@name"].Value = assetName;
            _getEntriesCommand.Parameters["@source"].Value = source;
            var results = new List<string>();
            using (var reader = _getEntriesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(reader.GetString(0));
                }
            }
            
            _transaction.Commit();
            return results.ToArray();
        }

        public AssetLookupResult[] GetAssetsWithId(string assetGuid, string source, int maxResults = 1)
        {
            _transaction = _db.BeginTransaction();
            _getMatchingAssetsCommand.Transaction = _transaction;
            _getMatchingAssetsCommand.Parameters["@guid"].Value = assetGuid;
            _getMatchingAssetsCommand.Parameters["@source"].Value = source;
            _getMatchingAssetsCommand.Parameters["@max"].Value = maxResults;
            var results = new AssetLookupResult[maxResults];
            using (var reader = _getMatchingAssetsCommand.ExecuteReader())
            {
                for (int i = 0; i < results.Length; i++)
                {
                    if (!reader.Read()) continue;
                    var nextResult = new AssetLookupResult
                    {
                        Source = reader.GetString(0),
                        Guid = reader.GetString(1),
                        BaseGameObject = reader.GetString(2),
                        TransformPath = reader.GetString(3).Split(';').Select(idx => Convert.ToInt32(idx)).ToArray(),
                        Property = reader.GetString(4),
                        IsCollection = reader.GetBoolean(5),
                        CollectionIndex = reader.GetInt32(6),
                        ComponentType = reader.GetString(7)
                    };
                    results[i] = nextResult;
                }
            }
            _transaction.Commit();
            return results;
        }

        private void CreateSQLCommands()
        {
            _getComponentTypeCommand = _db.CreateCommand();
            _getComponentTypeCommand.CommandText = "SELECT type FROM component_types WHERE id = @id;";
            _getComponentTypeCommand.Parameters.Add("@id", SqliteType.Text);

            _entryExistsCommand = _db.CreateCommand();
            _entryExistsCommand.CommandText =
                "SELECT COUNT(*) FROM assets WHERE asset_guid = @asset_guid AND source = @source";
            _entryExistsCommand.Parameters.Add("@asset_guid", SqliteType.Text);
            _entryExistsCommand.Parameters.Add("@source", SqliteType.Text);

            _getMatchingAssetsCommand = _db.CreateCommand();
            _getMatchingAssetsCommand.CommandText = 
                @"SELECT assets.source, assets.asset_guid, asset_locations.base_gameobject, asset_locations.transform_path, 
property_data.property_name, property_data.is_collection, property_data.collection_index, component_types.type FROM assets 
INNER JOIN asset_locations ON assets.location_id = asset_locations.id 
INNER JOIN property_data ON asset_locations.property_id = property_data.id 
INNER JOIN component_types ON property_data.component_id = component_types.id 

WHERE assets.source = @source AND assets.asset_guid = @guid LIMIT @max;";
            _getMatchingAssetsCommand.Parameters.Add("@source", SqliteType.Text);
            _getMatchingAssetsCommand.Parameters.Add("@guid", SqliteType.Text);
            _getMatchingAssetsCommand.Parameters.Add("@max", SqliteType.Integer);

            _getEntriesCommand = _db.CreateCommand();
            _getEntriesCommand.CommandText =
                "SELECT asset_guid FROM assets WHERE asset_name = @name AND source = @source";
            _getEntriesCommand.Parameters.Add("@name", SqliteType.Text);
            _getEntriesCommand.Parameters.Add("@source", SqliteType.Text);
        }
    }
}