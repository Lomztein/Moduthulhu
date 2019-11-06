using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Core.Bot;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lomztein.Moduthulhu.Plugins.Standard;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Plugin
{
    internal static class PluginLoader
    {
        private static Type[] _loadedPlugins;
        private static Type[] _standardPlugins = new Type[] { typeof(LoggerPlugin), typeof(CommandPlugin), typeof(StandardCommandsPlugin), typeof (AdministrationPlugin) };

        public static Type[] GetAllPlugins ()
        {
            Type[] all = new Type[_loadedPlugins.Length + _standardPlugins.Length];
            for (int i = 0; i < all.Length; i++)
            {
                if (i < _standardPlugins.Length)
                {
                    all[i] = _standardPlugins[i];
                }
                else
                {
                    all[i] = _loadedPlugins[i - (_standardPlugins.Length - 1)];
                }
            }
            return all;
        }

        public static bool IsStandard(Type pluginType) => _standardPlugins.Contains(pluginType);

        public static Type GetPluginType(string name) => GetAllPlugins ().Where ((x) => Framework.Plugin.CompactizeName(x) == name).FirstOrDefault ();

        public static string AssemblyPath => Bot.Core.DataDirectory + "/Plugins";

        public static PluginDependancyTree DependancyTree { get; private set; }

        public static void ReloadPluginAssemblies()
        {
            if (!Directory.Exists(AssemblyPath)) Directory.CreateDirectory(AssemblyPath);
            _loadedPlugins = AssemblyLoader.LoadAndExtractTypes<IPlugin>(AssemblyPath);
            DependancyTree = new PluginDependancyTree(_loadedPlugins);
        }
    }
}
