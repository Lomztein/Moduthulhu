using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    internal class IdentityKeyValueRepository<TIdentity, TKey, TValue>
    {
        private string _tableName;

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
            var res = db.ReadQuery("SELECT COUNT(*) AS count FROM information_schema.tables WHERE table.name = @name", new Dictionary<string, object>() { { "name", _tableName } });
            if ((int)res[0]["count"] == 0)
            {
                db.UpdateQuery("CREATE TABLE @name (identity bigint, key text, value text);", new Dictionary<string, object>() { { "name", _tableName } });
            }
        }

        public void Create(TIdentity identity, TKey key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            db.UpdateQuery("INSERT INTO @table VALUES (@identity, @key, @value)", new Dictionary<string, object>() { { "table", _tableName }, { "identity", identity }, { "key", key }, { "value", value } });
        }

        public TValue Read (TIdentity identity, TKey key)
        {
            IDatabaseConnector db = GetConnector();
            var res = db.ReadQuery("SELECT value FROM @name WHERE identity = @identity AND key = @key", new Dictionary<string, object>() { { "name", _tableName }, { "identity", identity }, { "key", key } });
            return res.Length == 0 ? default : (TValue)res.Single ().FirstOrDefault ().Value;
        }

        public void Update (TIdentity identity, TKey key, TValue value)
        {
            IDatabaseConnector db = GetConnector();
            db.UpdateQuery("UPDATE @table SET value = @value WHERE identity = @identity AND key = @key", new Dictionary<string, object>() { { "table", _tableName }, { "identity", identity }, { "key", key }, { "value", value } });
        }

        public void Delete (TIdentity identity, TKey key)
        {
            IDatabaseConnector db = GetConnector();
            db.UpdateQuery("DELETE FROM @table WHERE identity = @identity AND key = @key", new Dictionary<string, object>() { { "table", _tableName }, { "identity", identity }, { "key", key } });
        }
    }
}
