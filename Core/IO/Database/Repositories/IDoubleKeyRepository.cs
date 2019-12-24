using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public interface IDoubleKeyRepository<TIdentifier, TKey, TValue>
    {
        void Init();

        TValue GetValue(TIdentifier identifier, TKey key);

        void SetValue(TIdentifier identifier, TKey key, TValue value);
    }
}
