using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lomztein.Moduthulhu.Core
{
    internal class Filter<T>
    {
        private List<Predicate<T>> Filters { get; set; } = new List<Predicate<T>> ();

        internal Filter (params Predicate<T>[] filters) {
            Filters = filters.ToList ();
        }

        internal IEnumerable<T> FilterModules (IEnumerable<T> enumerable) {
            IEnumerable<T> result = new List<T> (enumerable);
            Filters.ForEach (x => result = result.Where (y => x(y)));
            return result;
        }
    }
}
