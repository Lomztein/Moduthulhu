using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class IdentityKeyJsonRepository
    {
        private readonly IdentityKeyValueRepository<string, string, string> _dataRepo;

        public IdentityKeyJsonRepository (string sourceTable)
        {
            _dataRepo = new IdentityKeyValueRepository<string, string, string>(sourceTable);
            _dataRepo.Init();
        }

        public T Get<T> (ulong id, string key)
        {
            string json = _dataRepo.Get(id.ToString (), key);
            if (json == null) { return default; }
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public void Set(ulong id, string key, object value)
        {
            _dataRepo.Set(id.ToString (), key, JsonConvert.SerializeObject (value));
        }
    }
}
