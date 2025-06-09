using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class DatabaseReader : DatabaseInterface
    {
        private SqliteTransaction _transaction;
        
        private SqliteCommand _getScriptTypeCommand;
        private SqliteCommand _entryExistsCommand;

        public DatabaseReader(string databasePath) : base(databasePath)
        {
            CreateSQLCommands();
        }

        public string GetScriptType(string scriptGuid)
        {
            _transaction = _db.BeginTransaction();
            _getScriptTypeCommand.Transaction = _transaction;
            _getScriptTypeCommand.Parameters["@id"].Value = scriptGuid;
            var res = (string)_getScriptTypeCommand.ExecuteScalar();
            _transaction.Commit();
            return !string.IsNullOrEmpty(res) ? res : "MonoBehaviour";
        }

        public bool EntryExistsForAsset(string assetGuid)
        {
            _transaction = _db.BeginTransaction();
            _entryExistsCommand.Transaction = _transaction;
            _entryExistsCommand.Parameters["@asset_guid"].Value = assetGuid;
            var res = (int?)_entryExistsCommand.ExecuteScalar();
            _transaction.Commit();
            return res > 0;
        }

        private void CreateSQLCommands()
        {
            _getScriptTypeCommand = _db.CreateCommand();
            _getScriptTypeCommand.CommandText = "SELECT type FROM script_types WHERE id = @id;";
            _getScriptTypeCommand.Parameters.Add("@id", SqliteType.Text);

            _entryExistsCommand = _db.CreateCommand();
            _entryExistsCommand.CommandText =
                "SELECT COUNT(*) FROM assets WHERE asset_guid = @asset_guid AND source = 'basegame'";
            _entryExistsCommand.Parameters.Add("@asset_guid", SqliteType.Integer);
        }
    }
}