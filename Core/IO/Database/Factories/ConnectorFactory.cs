using Lomztein.Moduthulhu.Core.IO.Database;
using Lomztein.Moduthulhu.Core.IO.Database.Connectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Factories
{
    public class GenericFactory
    {
        private Dictionary<string, Type> _supportedTypes;

        public static readonly GenericFactory SQL = new GenericFactory(new Dictionary<string, Type> {
            { "PostgreSQL", typeof(PostgreSQLDatabaseConnector) }
        });

        public static readonly GenericFactory Path = new GenericFactory(new Dictionary<string, Type>
        {
            { "Json", typeof (JsonLocalConnector) }
        });

        public GenericFactory (Dictionary<string, Type> supportedTypes)
        {
            _supportedTypes = supportedTypes;
        }

        public object Create (string type)
        {
            if (!_supportedTypes.ContainsKey (type))
            {
                throw new NotImplementedException($"The used factory does not support type '{type}'");
            }

            object connector = Activator.CreateInstance(_supportedTypes[type]);
            return connector;
        }
        public object Create() => Create(Database.GetDatabaseType ());
    }

    public class GenericFactory<T> : GenericFactory
    {
        public GenericFactory (Dictionary<string, Type> supportedTypes) : base(supportedTypes) { }

        public new T Create(string type) => Create(type);
        public new T Create() => Create();
    }
}
