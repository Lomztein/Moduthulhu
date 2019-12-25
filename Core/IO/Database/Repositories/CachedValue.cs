using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class CachedValue<T> : IValueRepository<T>
    {
        private readonly DoubleKeyJsonRepository _repo;
        private readonly ulong _identity;
        private readonly string _key;
        private readonly Func<T> _defaultValue;

        private T _value;
        private bool _dirty = true;
        private bool _isSet = false;

        public CachedValue(DoubleKeyJsonRepository repo, ulong identity, string key, Func<T> defaultValue)
        {
            _repo = repo;
            _identity = identity;
            _key = key;
            _defaultValue = defaultValue;
        }

        public T GetValue() {
            if (_dirty)
            {
                Cache();
            }
            return _value;
        }

        public void SetValue(T value)
        {
            _value = value;
            _isSet = true;
            Store();
        }

        public void MutateValue(Action<T> action)
        {
            action(_value);
            _isSet = true;
            Store();
        }

        public void Cache()
        {
            JToken obj = _repo.Get(_identity, _key);
            FromJson(obj);

            if (!_isSet)
            {
                _value = _defaultValue();
                Store();
            }
            _dirty = false;
        }
        public void Store()
        {
            _repo.Set(_identity, _key, ToJson ());
        }

        private JToken ToJson()
        {
            JObject obj = new JObject
            {
                { "Value", JToken.FromObject (_value) },
                { "IsSet", JToken.FromObject (_isSet) },
            };
            return obj;
        }

        private void FromJson(JToken obj)
        {
            if (IsSimple(obj))
            {
                Log.Data("Loaded simple CachedValue data.");
                if (obj == null)
                {
                    _value = default;
                    _isSet = false;
                }
                else
                {
                    _value = obj.ToObject<T>();
                    _isSet = true;
                }
            }
            else
            {
                _value = obj["Value"].ToObject<T>();
                _isSet = obj["IsSet"].ToObject<bool>();
            }
        }

        /// <summary>
        /// A "simple" object refers to the old format, that being just the value without any metadata.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool IsSimple (JToken token)
        {
            if (token is JObject obj)
            {
                return !obj.ContainsKey("Value");
            }
            return true;
        }

        public void SetDirty() => _dirty = true;

        public void Reset ()
        {
            _value = _defaultValue ();
            _isSet = false;
            Store();
        }
    }
}
