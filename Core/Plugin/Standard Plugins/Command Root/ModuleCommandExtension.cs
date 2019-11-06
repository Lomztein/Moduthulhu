using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard

{
    public interface IPluginCommand<T> {

        T ParentPlugin { get; set; }

    }

    public class PluginCommand<T> : AdvDiscordCommands.Framework.Command, IPluginCommand<T> where T : IPlugin {

        public T ParentPlugin { get; set; }

    }

    public class PluginCommandSet<T> : CommandSet, IPluginCommand<T> where T : IPlugin {

        public T ParentPlugin { get; set; }

        public override void Initialize() {
            foreach (AdvDiscordCommands.Framework.Command cmd in commandsInSet) {
                (cmd as PluginCommand<T>).ParentPlugin = ParentPlugin;
            }
            base.Initialize ();
        }

    }
}
