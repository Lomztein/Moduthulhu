using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Core.Bot;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Lomztein.Moduthulhu.Core.Plugin
{
    public static class PluginLoader
    {
        public static Type[] LoadedPlugins { get; private set; }

        public static string AssemblyPath => Bot.Core.DataDirectory + "/Modules";

        public static PluginDependancyTree DependancyTree { get; private set; }

        public static void ReloadPluginAssemblies()
        {
            if (!Directory.Exists(AssemblyPath)) Directory.CreateDirectory(AssemblyPath);
            LoadedPlugins = AssemblyLoader.LoadAndExtractTypes<IPlugin>(AssemblyPath);
            DependancyTree = new PluginDependancyTree(LoadedPlugins);
        }
    }
}
