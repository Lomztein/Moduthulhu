using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Critical]
    [Dependency ("Lomztein-Command Root")]
    [Descriptor ("Lomztein", "Plugin Manager", "Plugin to handle toggling of other plugins on a per-server basis.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    public class PluginManagerPlugin : PluginBase
    {
        public PluginManager Manager => GuildHandler.Plugins;
        private PluginManagerCommands _commandSet;

        public override void Initialize()
        {
            _commandSet = new PluginManagerCommands() { ParentPlugin = this };
            SendMessage("Lomztein-Command Root", "AddCommand", _commandSet);
        }

        public override void Shutdown()
        {
            SendMessage("Lomztein-Command Root", "RemoveCommand", _commandSet);
        }

        public bool AddPlugin(string pluginName) => Manager.AddPlugin(pluginName);

        public bool RemovePlugin(string pluginName) => Manager.RemovePlugin(pluginName);

        public void ReloadPlugins() => Manager.ReloadPlugins();

        public Type[] GetAvailablePlugins() => PluginLoader.GetPlugins();

        public IPlugin[] GetActivePlugins() => Manager.GetActivePlugins();
    }
}
