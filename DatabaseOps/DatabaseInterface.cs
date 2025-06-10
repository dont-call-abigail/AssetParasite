using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public abstract class DatabaseInterface : IDisposable
    {
        protected SqliteConnection _db;

        protected DatabaseInterface(string databasePath)
        {
            try
            {
                bool isNew = !File.Exists(databasePath);
                _db = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Pooling = false,
                    ForeignKeys = false
                }.ConnectionString);
                _db.Open();

                if (isNew)
                {
                    var command = _db.CreateCommand();
                    command.CommandText = Resources.Init;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR OPENING ASSET REFERENCE DATABASE at {databasePath}");
                throw;
            }
        }

        public void Dispose()
        {
            _db?.Close();
            _db?.Dispose();
        }
    }
}