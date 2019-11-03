using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class DataManager
    {
        private IdentityKeyValueRepository<string, string, string> _dataRepo;
        private ulong _identifierId;

        public DataManager (ulong identifierId)
        {
            _identifierId = identifierId;
            _dataRepo = new IdentityKeyValueRepository<string, string, string>("plugindata");
            _dataRepo.Init();
        }

        public T Get<T> (string key)
        {
            string json = _dataRepo.Read(_identifierId.ToString (), key);
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public void Set(string key, object value)
        {
            _dataRepo.InsertOrUpdate(_identifierId.ToString (), key, JsonConvert.SerializeObject (value));
        }
    }
}
