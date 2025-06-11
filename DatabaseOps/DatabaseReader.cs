using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class DatabaseReader : DatabaseInterface
    {
        private SqliteTransaction _transaction;
        
        private SqliteCommand _getComponentTypeCommand;
        private SqliteCommand _entryExistsCommand;
        private SqliteCommand _getMatchingAssetsCommand;

        public DatabaseReader(string databasePath) : base(databasePath)
        {
            CreateSQLCommands();
        }

        public string GetScriptType(string scriptGuid)
        {
            _transaction = _db.BeginTransaction();
            _getComponentTypeCommand.Transaction = _transaction;
            _getComponentTypeCommand.Parameters["@id"].Value = scriptGuid;
            var res = (string)_getComponentTypeCommand.ExecuteScalar();
            _transaction.Commit();
            return !string.IsNullOrEmpty(res) ? res : "MonoBehaviour";
        }

        public bool EntryExistsForAsset(string assetGuid, string source)
        {
            _transaction = _db.BeginTransaction();
            _entryExistsCommand.Transaction = _transaction;
            _entryExistsCommand.Parameters["@asset_guid"].Value = assetGuid;
            _entryExistsCommand.Parameters["@source"].Value = source;
            var res = (int?)_entryExistsCommand.ExecuteScalar();
            _transaction.Commit();
            return res > 0;
        }

        public void GetReferencedAssets(string assetName, string source)
        {
            _transaction = _db.BeginTransaction();
        }

        private void CreateSQLCommands()
        {
            _getComponentTypeCommand = _db.CreateCommand();
            _getComponentTypeCommand.CommandText = "SELECT type FROM component_types WHERE id = @id;";
            _getComponentTypeCommand.Parameters.Add("@id", SqliteType.Text);

            _entryExistsCommand = _db.CreateCommand();
            _entryExistsCommand.CommandText =
                "SELECT COUNT(*) FROM assets WHERE asset_guid = @asset_guid AND source = @source";
            _entryExistsCommand.Parameters.Add("@asset_guid", SqliteType.Integer);
            _entryExistsCommand.Parameters.Add("@source", SqliteType.Text);

            _getMatchingAssetsCommand = _db.CreateCommand();

        }
    }
}