using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Plugin;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModuthulhuCore.Core.Plugin.Standard
{
    [Critical]
    [Dependency ("Lomztein-Command Root")]
    [Descriptor ("Lomztein", "Plugin Manager", "Plugin to handle toggling of other plugins on a per-server basis.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    public class PluginManagerPlugin : PluginBase
    {
        private PluginManager Manager => GuildHandler.Plugins;

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void AddPlugin(string pluginName) => Manager.AddPlugin(pluginName);

        public void RemovePlugin(string pluginName) => Manager.RemovePlugin(pluginName);

        public void ReloadPlugins() => Manager.ReloadPlugins();

        public Type[] GetAvailablePlugins() => PluginLoader.GetPlugins();

        public IPlugin[] GetActivePlugins() => Manager.GetActivePlugins();
    }
}
