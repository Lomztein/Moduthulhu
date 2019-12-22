using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard 
{
    [Descriptor("Lomztein", "Configuration", "Plugin that exposes plugin configuration functions as commands.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Core/Plugin/Standard%20Plugins/Configuration")]
    [Dependency ("Lomztein-Command Root")]
    [Critical]
    public class ConfigurationPlugin : PluginBase
    {
        private ConfigCommandSet _configCommands;

        public override void Initialize()
        {
            _configCommands = new ConfigCommandSet();
            SendMessage("Lomztein-Command Root", "AddCommand", _configCommands);
        }

        public override void PostInitialize()
        {
            var configInfos = GuildHandler.Config.GetConfigInfo();
            var grouped = configInfos.GroupBy(x => x.Identifier);

            foreach (var group in grouped)
            {
                var overloadGroups = group.GroupBy(x => x.Name);

                string name = "Core Configuration";
                string desc = "Configuration for core components of the bot.";

                Type source = PluginLoader.GetPlugin (group.Key);
                if (source != null)
                {
                    name = Plugin.GetName(source);
                    desc = $"Configuration options for plugin {name}.";
                }

                var commands = overloadGroups.Select(x => new ConfigCommand(x.ToArray(), name, desc));
                _configCommands.AddCommands(commands.ToArray());
            }
            _configCommands.InitCommands();

            foreach (var cmd in _configCommands.GetCommands())
            {
                AddPluginStateAttribute("Added configuration options", "Removed configuration options", cmd.Name, cmd.GetCommand(GuildHandler.GuildId));
            }
        }

        public override void Shutdown()
        {
            SendMessage("Lomztein-Command Root", "RemoveCommand", _configCommands);
        }
    }
}
