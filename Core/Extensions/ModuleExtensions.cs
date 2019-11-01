using Lomztein.Moduthulhu.Core;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    public static class PluginExtensions
    {
        public static string CompactizeName (this Plugin.Framework.IPlugin plugin) => (plugin.Author + "_" + plugin.Name);

        public static (string name, string author) DecompactizeModuleName(this string name) {
            string [ ] split = name.Split ('_');
            return (split[1], split[0]);
        }

        public static void Log(this IPlugin plugin, string text) {
            Core.Log.Write (Core.Log.GetColor (Core.Log.Type.MODULE), $"{plugin.CompactizeName ()} - { plugin.GuildHandler.GetGuild ().Name}", text);
        }

        public static string[] GetDependencyNames(this Plugin.Framework.IPlugin module) => module.GetType ().GetCustomAttributes (typeof (DependencyAttribute), false).Cast<DependencyAttribute> ().Select (x => x.DependencyName).ToArray ();

    }
}
