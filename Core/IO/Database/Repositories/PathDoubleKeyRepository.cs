using Lomztein.Moduthulhu.Core.IO.Database.Connectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class PathDoubleKeyRepository<TValue> : IDoubleKeyRepository<TValue>
    {
        private readonly string _table;
        private readonly IPathConnector _connector;

        public PathDoubleKeyRepository (string table, IPathConnector connector)
        {
            _table = table;
            _connector = connector;
        }

        public void Init() { }

        private IPathConnector GetConnector() => _connector;

        private string GetPath(ulong identifier, string key)
            => $"{_table}/{identifier}/{key}".Replace ('.', '/');

        public TValue GetValue(ulong identifier, string key)
        {
            return GetConnector ().GetValue<TValue>(GetPath(identifier, key));
        }

        public void SetValue(ulong identifier, string key, TValue value)
        {
            GetConnector().SetValue(GetPath(identifier, key), value);
        }

        public TValue[] GetAllValues(ulong identifier, string prefix)
        {
            return GetConnector().GetAllValues<TValue>(GetPath(identifier, prefix));
        }
    }
}
