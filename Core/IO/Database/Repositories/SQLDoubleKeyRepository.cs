using Lomztein.Moduthulhu.Core.IO.Database.Factories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class SQLDoubleKeyRepository<TValue> : IDoubleKeyRepository<TValue>
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

        public TValue GetValue (ulong identifier, string key)
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery($"SELECT value FROM {_tableName} WHERE identifier = @identifier AND key = @key", new Dictionary<string, object> { { "@identifier", identifier.ToString (CultureInfo.InvariantCulture) }, { "@key", key } });
            return res.Length == 0 ? default : (TValue)res.Single ().FirstOrDefault ().Value;
        }

        public void SetValue (ulong identifier, string key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            string query = $"INSERT INTO {_tableName} VALUES (@identifier, @key, @value) ON CONFLICT ON CONSTRAINT {_tableName}identkey DO UPDATE SET value = @value WHERE {_tableName}.identifier = @identifier AND {_tableName}.key = @key";
            db.UpdateQuery(query, new Dictionary<string, object> { { "@identifier", identifier.ToString(CultureInfo.InvariantCulture) }, { "@key", key }, { "@value", value } });
        }

        public TValue[] GetAllValues(ulong identifier, string prefix)
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery($"SELECT value FROM {_tableName} WHERE identifier = @identifier AND key LIKE @prefix",
                new Dictionary<string, object> { { "@identifier", identifier.ToString(CultureInfo.InvariantCulture) }, { "@prefix", $"{prefix}%" } });
            return res.Length == 0 ? default : res.Select (x => (TValue)x.Values.FirstOrDefault()).ToArray();
        }
    }
}
