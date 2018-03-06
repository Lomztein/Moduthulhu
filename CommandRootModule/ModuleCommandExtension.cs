using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CommandRoot
{
    public class ModuleCommand<T> : Command where T : IModule {

        public T parentModule;

    }

    public class ModuleCommandSet<T> : CommandSet where T : IModule {

        public T parentModule;

        public override void Initialize() {
            foreach (Command cmd in commandsInSet) {
                (cmd as ModuleCommand<T>).parentModule = parentModule;
            }
            base.Initialize ();
        }

    }
}
