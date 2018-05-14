using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class ModuleExtensions
    {
        public static string CompactizeName (this IModule module) => (module.Author + "_" + module.Name);

        public static (string name, string author) DecompactizeModuleName(this string name) {
            string [ ] split = name.Split ('_');
            return (split[1], split[0]);
        }

        public static bool ContainsPrerequisites (this IModule module, IEnumerable<IModule> list) {

            foreach (string required in module.RequiredModules) {

                string name = required.DecompactizeModuleName ().name;
                string author = required.DecompactizeModuleName ().author;

                IModule prerequisite = list.FirstOrDefault (x => x.Name == name && x.Author == author);
                bool enabled = prerequisite == null ? false : prerequisite.ContainsPrerequisites (list);

                if (!enabled) {
                    Log.Write (Log.Type.CRITICAL, $"Module {module.CompactizeName ()} can't load due to missing module prerequisite {required}.");
                    return false;
                } else
                    return true;
            }

            return true;
        }
    }
}
