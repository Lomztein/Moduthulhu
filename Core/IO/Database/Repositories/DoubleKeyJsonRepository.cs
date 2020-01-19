using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class DoubleKeyJsonRepository
    {
        private readonly IDoubleKeyRepository<string> _dataRepo;

        public DoubleKeyJsonRepository (string sourceTable)
        {
            _dataRepo = Factories.DoubleKeyRepositoryFactory.Create<string, string, string> (sourceTable);
            _dataRepo.Init();
        }

        public JToken Get (ulong id, string key)
        {
            string json = _dataRepo.GetValue(id, key);
            JToken obj = string.IsNullOrEmpty (json) ? null : JToken.Parse(json);
            return obj;
        }

        public void Set(ulong id, string key, JToken value)
        {
            _dataRepo.SetValue(id, key, value.ToString (Formatting.None));
        }

        public JToken[] GetAll (ulong id, string keyPrefix)
        {
            string[] values = _dataRepo.GetAllValues(id, keyPrefix);
            return values.Select(x => string.IsNullOrEmpty(x) ? null : JToken.Parse(x)).ToArray();
        }
    }
}
