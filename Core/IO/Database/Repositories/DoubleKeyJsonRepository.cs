using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class DoubleKeyJsonRepository
    {
        private readonly IDoubleKeyRepository<string, string, string> _dataRepo;

        public DoubleKeyJsonRepository (string sourceTable)
        {
            _dataRepo = Factories.DoubleKeyRepositoryFactory.Create<string, string, string> (sourceTable);
            _dataRepo.Init();
        }

        public T Get<T> (ulong id, string key)
        {
            string json = _dataRepo.GetValue(id.ToString (CultureInfo.InvariantCulture), key);
            if (json == null) { return default; }
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public void Set(ulong id, string key, object value)
        {
            _dataRepo.SetValue(id.ToString (CultureInfo.InvariantCulture), key, JsonConvert.SerializeObject (value));
        }
    }
}
