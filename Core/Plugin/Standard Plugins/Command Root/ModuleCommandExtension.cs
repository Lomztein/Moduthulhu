using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Plugins.Framework;

namespace Lomztein.Moduthulhu.Plugins.Standard

{
    public interface IPluginCommand<T> {

        T ParentPlugin { get; set; }

    }

    public class PluginCommand<T> : Command, IPluginCommand<T> where T : IPlugin {

        public T ParentPlugin { get; set; }

    }

    public class PluginCommandSet<T> : CommandSet, IPluginCommand<T> where T : IPlugin {

        public T ParentPlugin { get; set; }

        public override void Initialize() {
            foreach (Command cmd in _commandsInSet) {
                if (cmd is PluginCommand<T> child)
                {
                    child.ParentPlugin = ParentPlugin;
                }
            }
            if (_defaultCommand is IPluginCommand<T> pluginCmd)
            {
                pluginCmd.ParentPlugin = ParentPlugin;
            }
            base.Initialize ();
        }

    }
}
