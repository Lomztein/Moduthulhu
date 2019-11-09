using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.Bot;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lomztein.Moduthulhu.Plugins.Standard;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Plugins
{
    internal static class PluginLoader
    {
        private static Type[] _loadedPlugins;
        private static Type[] _standardPlugins = new Type[] { typeof (PluginManagerPlugin), typeof(LoggerPlugin), typeof(CommandPlugin), typeof(StandardCommandsPlugin), typeof (AdministrationPlugin) };

        private static Type[] GetAllPlugins ()
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

        private static Type[] _orderedPlugins;
        public static Type[] GetPlugins() => _orderedPlugins;
        public static Type GetPlugin(int id) => GetPlugins()[id];
        public static int GetID(Type plugin) => GetPlugins().ToList().IndexOf(plugin); 

        public static bool IsStandard(Type pluginType) => _standardPlugins.Contains(pluginType);

        public static Type GetPluginType(string name) => Framework.Plugin.Find(GetAllPlugins (), name);

        public static string AssemblyPath => BotCore.DataDirectory + "/Plugins";

        public static PluginDependancyTree DependancyTree { get; private set; }

        public static void ReloadPluginAssemblies()
        {
            if (!Directory.Exists(AssemblyPath)) Directory.CreateDirectory(AssemblyPath);
            _loadedPlugins = AssemblyLoader.LoadAndExtractTypes<IPlugin>(AssemblyPath);
            DependancyTree = new PluginDependancyTree(GetAllPlugins ());
            _orderedPlugins = DependancyTree.Order(GetAllPlugins ()).ToArray();
        }
    }
}
