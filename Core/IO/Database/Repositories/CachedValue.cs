using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class CachedValue<T> : IValueRepository<T>
    {
        private IdentityKeyJsonRepository _repo;
        private ulong _identity;
        private string _key;
        private Func<T> _defaultValue;

        private T _value;
        private bool _dirty = true;

        public CachedValue (IdentityKeyJsonRepository repo, ulong identity, string key, Func<T> defaultValue)
        {
            _repo = repo;
            _identity = identity;
            _key = key;
            _defaultValue = defaultValue;
        }

        public T GetValue () {
            if (_dirty)
            {
                Cache();
            }
            return _value;
        }

        public void SetValue(T value)
        {
            _value = value;
            Store();
        }

        public void MutateValue(Action<T> action)
        {
            action(_value);
            Store();
        }

        public void Cache ()
        {
            _value = _repo.Get<T>(_identity, _key);
            if (Equals (_value, default(T)))
            {
                _value = _defaultValue();
                Store();
            }
            _dirty = false;
        }
        public void Store ()
        {
            _repo.Set(_identity, _key, _value);
        }

        public void SetDirty() => _dirty = true;

        public void Reset ()
        {
            _value = default;
        }
    }
}
