using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    internal class IdentityKeyValueRepository<TIdentifier, TKey, TValue>
    {
        private readonly string _tableName;

        public IdentityKeyValueRepository (string tableName)
        {
            _tableName = tableName;
        }

        public void Init ()
        {
            CreateTableIfAbsent();
        }

        private IDatabaseConnector GetConnector() => new PostgreSQLDatabaseConnector();

        private void CreateTableIfAbsent ()
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery("SELECT 1 FROM information_schema.tables WHERE table_name = @table", new Dictionary<string, object>() { { "@table", _tableName } });

            try
            {
                if (res.Length == 0)
                {
                    db.UpdateQuery($"CREATE TABLE {_tableName} (identifier text, key text, value text, CONSTRAINT {_tableName}identkey UNIQUE (identifier, key));", new Dictionary<string, object>());
                }
            }catch (NpgsqlException e)
            {
                Log.Write(e);
            }
        }

        public TValue Get (TIdentifier identifier, TKey key)
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery($"SELECT value FROM {_tableName} WHERE identifier = @identifier AND key = @key", new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key } });
            return res.Length == 0 ? default : (TValue)res.Single ().FirstOrDefault ().Value;
        }

        public void Set (TIdentifier identifier, TKey key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            string query = $"INSERT INTO {_tableName} VALUES (@identifier, @key, @value) ON CONFLICT ON CONSTRAINT {_tableName}identkey DO UPDATE SET value = @value WHERE {_tableName}.identifier = @identifier AND {_tableName}.key = @key";
            db.UpdateQuery(query, new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key }, { "@value", value } });
        }
    }
}
