using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.Meta.Commands;
using System;

namespace Lomztein.Moduthulhu.Modules.Meta
{
    public class ModuleManagerModule : ModuleBase {

        public override string Name => "Module Manager";
        public override string Description => "Manually keep track of and manage modules.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override string [ ] RequiredModules => new string [ ] { "Lomztein_Command Root" };

        private ModuleManagerCommandSet moduleCommands = new ModuleManagerCommandSet ();

        public override void Initialize() {
            moduleCommands.ParentModule = this;
            ParentModuleHandler.GetModule<CommandRoot.CommandRootModule> ().commandRoot.AddCommands (moduleCommands);
        }

        public override void Shutdown() {
            ParentModuleHandler.GetModule<CommandRoot.CommandRootModule> ().commandRoot.RemoveCommands (moduleCommands);
        }

    }
}
