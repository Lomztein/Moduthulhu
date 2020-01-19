using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public class CachedArray<T>
    {
        private DoubleKeyJsonRepository _repo;
        private ulong _identifier;
        private string _prefix;

        public CachedArray (DoubleKeyJsonRepository repo, ulong identifier, string prefix)
        {
            _repo = repo;
            _identifier = identifier;
            _prefix = prefix;
        }

        public T[] GetValue()
        {
            return _repo.GetAll(_identifier, _prefix).Select(x => x["Value"].ToObject<T>()).ToArray();
        }
    }
}
