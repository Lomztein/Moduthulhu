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
        private static Type[] _standardPlugins = new [] { 
            typeof (PluginManagerPlugin),
            typeof (LoggerPlugin),
            typeof (StandardCommandsPlugin),
            typeof (AdministrationPlugin),
            typeof (ConfigurationPlugin),
            typeof (ConsentPlugin),
            typeof (CommandPlugin),
        };

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
                    all[i] = _loadedPlugins[i - (_standardPlugins.Length)];
                }
            }
            return all;
        }

        private static Type[] _orderedPlugins;
        public static Type[] GetPlugins() => _orderedPlugins;

        public static bool IsStandard(Type pluginType) => _standardPlugins.Contains(pluginType);

        public static Type GetPlugin(string name) => Plugin.Find(GetPlugins (), name);

        private static string IncludedPath => BotCore.BaseDirectory + "/IncludedPlugins.dll"; // Included plugins points to a specific files with plugins that aren't standard, but are a part of the basic version of the bot.
        // IncludedPath is temporary untill a more robust plugin build pipeline can be figured out. Until then, this is specifically designed to allow for the plugins included in this project to be included in the Docker Image as well.
        public static string ThirdPartyPath => BotCore.DataDirectory + "/Plugins"; // AssemblyPath points to a directory that contains all any and all potential third-party plugins.

        public static PluginDependancyTree DependancyTree { get; private set; }

        public static void ReloadPluginAssemblies()
        {
            if (!Directory.Exists(ThirdPartyPath)) Directory.CreateDirectory(ThirdPartyPath);
            var firstParty = Array.Empty<Type>();
            
            if (File.Exists (IncludedPath))
            {
                Log.Write(Log.Type.PLUGIN, "Loading first party plugins..");
                firstParty = AssemblyLoader.ExtractTypes<IPlugin>(AssemblyLoader.LoadAssembly(IncludedPath));
            }
            else
            {
                Log.Write(Log.Type.PLUGIN, "There are no first party plugins to load.");
            }

            var thirdParty = AssemblyLoader.LoadAndExtractTypes<IPlugin>(ThirdPartyPath);

            List<Type> allParties = new List<Type>();
            allParties.AddRange(firstParty);
            allParties.AddRange(thirdParty);
            _loadedPlugins = allParties.ToArray();

            DependancyTree = new PluginDependancyTree(GetAllPlugins ());
            _orderedPlugins = DependancyTree.Order(GetAllPlugins ()).ToArray();
        }
    }
}
