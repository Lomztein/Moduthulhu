using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Command
{
    public interface IModuleCommand<T> {

        T ParentModule { get; set; }

    }

    public class ModuleCommand<T> : AdvDiscordCommands.Framework.Command, IModuleCommand<T> where T : IPlugin {

        public T ParentModule { get; set; }

    }

    public class ModuleCommandSet<T> : CommandSet, IModuleCommand<T> where T : IPlugin {

        public T ParentModule { get; set; }

        public override void Initialize() {
            foreach (AdvDiscordCommands.Framework.Command cmd in commandsInSet) {
                (cmd as ModuleCommand<T>).ParentModule = ParentModule;
            }
            base.Initialize ();
        }

    }
}
