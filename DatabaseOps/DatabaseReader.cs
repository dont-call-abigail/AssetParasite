using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class DatabaseReader : DatabaseInterface
    {
        private SqliteTransaction _transaction;
        
        private SqliteCommand _getScriptTypeCommand;

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

        private void CreateSQLCommands()
        {
            _getScriptTypeCommand = _db.CreateCommand();
            _getScriptTypeCommand.CommandText = "SELECT type FROM script_types WHERE id = @id;";
            _getScriptTypeCommand.Parameters.Add("@id", SqliteType.Text);
        }
    }
}