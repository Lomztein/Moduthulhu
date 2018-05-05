using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CommandRoot
{
    public interface IModuleCommand<T> {

        T ParentModule { get; set; }

    }

    public class ModuleCommand<T> : Command, IModuleCommand<T> where T : IModule {

        public T ParentModule { get; set; }

    }

    public class ModuleCommandSet<T> : CommandSet, IModuleCommand<T> where T : IModule {

        public T ParentModule { get; set; }

        public override void Initialize() {
            foreach (Command cmd in commandsInSet) {
                (cmd as ModuleCommand<T>).ParentModule = ParentModule;
            }
            base.Initialize ();
        }

    }
}
