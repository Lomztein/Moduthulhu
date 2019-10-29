using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Plugin
{
    internal class PluginFilter
    {
        private List<Predicate<IPlugin>> Filters { get; set; } = new List<Predicate<IPlugin>> ();

        internal PluginFilter (params Predicate<IPlugin>[] filters) {
            Filters = filters.ToList ();
        }

        internal IEnumerable<IPlugin> FilterModules (IEnumerable<IPlugin> modules) {
            IEnumerable<IPlugin> result = modules;
            Filters.ForEach (x => result = result.Where (y => x(y)));
            return result;
        }
    }
}
