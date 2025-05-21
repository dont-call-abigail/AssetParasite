using System;
using Microsoft.Data.Sqlite;

namespace DatabaseOps
{
    public class AssetRefDatabase
    {
        private SqliteConnection db;

        public AssetRefDatabase(string databasePath)
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
                db.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR OPENING ASSET REFERENCE DATABASE at {databasePath}");
                throw;
            }
        }

        public int InsertGameAsset()
        {
            
        }

        public int InsertModAsset()
        {
            
        }

        public bool RegisterComponentType(int id, string type)
        {
            
        }

        public int RegisterScriptType(string type)
        {
            
        }
    }
}