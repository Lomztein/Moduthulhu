using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration.Management.Adders
{
    public interface IConfigEnumerator : IConfigModifier
    {
        void AddTo(dynamic enumerable, List<dynamic> objects);

        void RemoveFrom(dynamic enumerable, int index);

        string ListToString(dynamic enumerable);

        void Change(dynamic enumerable, dynamic newObject, int index);
    }
}
