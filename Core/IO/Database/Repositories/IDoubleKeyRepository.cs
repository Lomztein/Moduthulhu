using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public interface IDoubleKeyRepository<TValue>
    {
        void Init();

        TValue GetValue(ulong identifier, string key);

        TValue[] GetAllValues(ulong identifier, string prefix);

        void SetValue(ulong identifier, string key, TValue value);
    }
}
