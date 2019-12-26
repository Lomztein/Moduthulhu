using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Critical]
    [Dependency ("Moduthulhu-Command Root")]
    [Descriptor ("Moduthulhu", "Plugin Manager", "Plugin to handle toggling of other plugins on a per-server basis.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Core/Plugin/Standard%20Plugins/Plugin%20Manager")]
    public class PluginManagerPlugin : PluginBase
    {
        public PluginManager Manager => GuildHandler.Plugins;
        private PluginManagerCommands _commandSet;

        public override void Initialize()
        {
            _commandSet = new PluginManagerCommands { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", _commandSet);
        }

        public override void Shutdown()
        {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _commandSet);
        }

        public bool AddPlugin(string pluginName)
        {
            bool value = Manager.AddPlugin(pluginName);
            Manager.ReloadPlugins();
            return value;
        }

        public bool RemovePlugin(string pluginName)
        {
            bool value = Manager.RemovePlugin(pluginName);
            Manager.ReloadPlugins();
            return value;
        }


        public void ReloadPlugins() => Manager.ReloadPlugins();

        public static Type[] GetAvailablePlugins() => PluginLoader.GetPlugins();

        public IPlugin[] GetActivePlugins() => Manager.GetActivePlugins();
    }
}
