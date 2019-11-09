using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Plugins
{
    internal class TypeFilter
    {
        private List<Predicate<Type>> Filters { get; set; } = new List<Predicate<Type>> ();

        internal TypeFilter (params Predicate<Type>[] filters) {
            Filters = filters.ToList ();
        }

        internal IEnumerable<Type> FilterModules (IEnumerable<Type> modules) {
            IEnumerable<Type> result = modules;
            Filters.ForEach (x => result = result.Where (y => x(y)));
            return result;
        }
    }
}
