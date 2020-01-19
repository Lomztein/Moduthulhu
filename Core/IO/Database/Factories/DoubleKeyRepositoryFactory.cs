using Lomztein.Moduthulhu.Core.IO.Database.Connectors;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Factories
{
    public static class DoubleKeyRepositoryFactory
    {
        private static readonly Dictionary<string, Type> _repos = new Dictionary<string, Type> {
            { "SQL", typeof (SQLDoubleKeyRepository<>) },
            { "Path", typeof (PathDoubleKeyRepository<>) }
        };

        private static readonly Dictionary<string, GenericFactory> _factories = new Dictionary<string, GenericFactory> {
            { "SQL", GenericFactory.SQL },
            { "Path", GenericFactory.Path }
        };

        // I did barf a little when writing this thanks for asking.

        public static IDoubleKeyRepository<TValue> Create<TValue> (string table, string typeString)
        {
            string group = Database.GetDatabaseGroup(typeString);
            string type = Database.GetDatabaseType(typeString);

            if (!_repos.ContainsKey (group))
            {
                throw new NotImplementedException($"The double key repository group '{group} is not supported.");
            }

            IDoubleKeyRepository<TValue> repo = null;
            Type groupType = _repos[group].MakeGenericType(typeof(TValue));
            repo = Activator.CreateInstance(groupType, table, _factories[group].Create(type)) as IDoubleKeyRepository<TValue>;

            repo.Init();
            return repo;
        }

        public static IDoubleKeyRepository<TValue> Create<TIdentifier, TKey, TValue>(string table)
            => Create<TValue>(table, Database.GetDatabaseTypeString());
    }
}
