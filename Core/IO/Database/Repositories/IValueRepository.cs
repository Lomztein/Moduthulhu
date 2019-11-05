using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Repositories
{
    public interface IValueRepository<T>
    {
        T GetValue();

        void SetValue(T value);
    }
}
