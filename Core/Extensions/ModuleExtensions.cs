using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lomztein.Moduthulhu.Cross;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class ModuleExtensions
    {
        public static string CompactizeName (this IModule module) => (module.Author + "_" + module.Name);

        public static (string name, string author) DecompactizeModuleName(this string name) {
            string [ ] split = name.Split ('_');
            return (split[1], split[0]);
        }

        public static void Log(this IModule module, string text) {
            Cross.Log.Write (Cross.Log.GetColor (Cross.Log.Type.MODULE), $"{module.CompactizeName ()} - { module.ParentShard.BotClient.Name} - S{ module.ParentShard.ShardId}/{ module.ParentShard.BotClient.TotalShards}", text);
        }

        public static string[] GetDependencyNames(this IModule module) => module.GetType ().GetCustomAttributes (typeof (DependencyAttribute), false).Cast<DependencyAttribute> ().Select (x => x.DependencyName).ToArray ();

    }
}
