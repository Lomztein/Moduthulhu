using Lomztein.Moduthulhu.Core.IO.Database.Connectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class PathDoubleKeyRepository<TIdentifier, TKey, TValue> : IDoubleKeyRepository<TIdentifier, TKey, TValue>
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

        private string GetPath(TIdentifier identifier, TKey key)
            => $"{_table}/{identifier}/{key}".Replace ('.', '/');

        public TValue GetValue(TIdentifier identifier, TKey key)
        {
            return GetConnector ().GetValue<TValue>(GetPath(identifier, key));
        }

        public void SetValue(TIdentifier identifier, TKey key, TValue value)
        {
            GetConnector().SetValue(GetPath(identifier, key), value);
        }
    }
}
