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

        public CachedValue (IdentityKeyJsonRepository repo, ulong identity, string key, Func<T> defaultValue)
        {
            _repo = repo;
            _identity = identity;
            _key = key;
            _defaultValue = defaultValue;
        }

        public T GetValue () {
            if (_value == null)
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

        public void Cache ()
        {
            _value = _repo.Get<T>(_identity, _key);
            if (_value == null)
            {
                _value = _defaultValue();
                Store();
            }
        }

        private void Store ()
        {
            _repo.Set(_identity, _key, _value);
        }
    }
}
