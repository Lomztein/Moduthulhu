using Lomztein.Moduthulhu.Core.IO.Database.Factories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    internal class SQLDoubleKeyRepository<TIdentifier, TKey, TValue> : IDoubleKeyRepository<TIdentifier, TKey, TValue>
    {
        private string _tableName;
        private IDatabaseConnector _connector;

        public SQLDoubleKeyRepository (string table, IDatabaseConnector connector)
        {
            _tableName = table;
            _connector = connector;
        } 

        public void Init ()
        {
            GetConnector().CreateTable(_tableName, $"CREATE TABLE {_tableName} (identifier text, key text, value text, CONSTRAINT {_tableName}identkey UNIQUE (identifier, key));");
        }

        private IDatabaseConnector GetConnector() => _connector;

        public TValue GetValue (TIdentifier identifier, TKey key)
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery($"SELECT value FROM {_tableName} WHERE identifier = @identifier AND key = @key", new Dictionary<string, object> { { "@identifier", identifier }, { "@key", key } });
            return res.Length == 0 ? default : (TValue)res.Single ().FirstOrDefault ().Value;
        }

        public void SetValue (TIdentifier identifier, TKey key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            string query = $"INSERT INTO {_tableName} VALUES (@identifier, @key, @value) ON CONFLICT ON CONSTRAINT {_tableName}identkey DO UPDATE SET value = @value WHERE {_tableName}.identifier = @identifier AND {_tableName}.key = @key";
            db.UpdateQuery(query, new Dictionary<string, object> { { "@identifier", identifier }, { "@key", key }, { "@value", value } });
        }
    }
}
