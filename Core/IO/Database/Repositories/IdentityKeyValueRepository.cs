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
            GetConnector().CreateTable(_tableName, $"CREATE TABLE {_tableName} (identifier text, key text, value text, CONSTRAINT {_tableName}identkey UNIQUE (identifier, key));");
        }

        private static IDatabaseConnector GetConnector() => new PostgreSQLDatabaseConnector();

        public TValue Get (TIdentifier identifier, TKey key)
        {
            Log.Write(Log.Type.DATA, $"Querying database table {_tableName} for identifier {identifier} and key {key}.");
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery($"SELECT value FROM {_tableName} WHERE identifier = @identifier AND key = @key", new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key } });
            return res.Length == 0 ? default : (TValue)res.Single ().FirstOrDefault ().Value;
        }

        public void Set (TIdentifier identifier, TKey key, TValue value)
        {
            Log.Write(Log.Type.DATA, $"Querying database table {_tableName} to set value at identifier {identifier} and key {key} to {value}.");
            IDatabaseConnector db = GetConnector();
            string query = $"INSERT INTO {_tableName} VALUES (@identifier, @key, @value) ON CONFLICT ON CONSTRAINT {_tableName}identkey DO UPDATE SET value = @value WHERE {_tableName}.identifier = @identifier AND {_tableName}.key = @key";
            db.UpdateQuery(query, new Dictionary<string, object>() { { "@identifier", identifier }, { "@key", key }, { "@value", value } });
        }
    }
}
