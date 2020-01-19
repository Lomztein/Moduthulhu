using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Connectors
{
    public interface IPathConnector
    {
        T GetValue<T>(string path);

        T[] GetAllValues<T>(string prefix);

        void SetValue(string path, object value);
    }
}
