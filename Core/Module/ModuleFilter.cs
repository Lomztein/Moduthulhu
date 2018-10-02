using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Module
{
    internal class ModuleFilter
    {
        private List<Predicate<IModule>> Filters { get; set; } = new List<Predicate<IModule>> ();

        internal ModuleFilter (params Predicate<IModule>[] filters) {
            Filters = filters.ToList ();
        }

        internal IEnumerable<IModule> FilterModules (IEnumerable<IModule> modules) {
            IEnumerable<IModule> result = modules;
            Filters.ForEach (x => result = result.Where (y => x(y)));
            return result;
        }
    }
}
