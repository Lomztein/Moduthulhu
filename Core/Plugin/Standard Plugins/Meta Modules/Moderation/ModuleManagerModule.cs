using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.Meta.Commands;
using System;

namespace Lomztein.Moduthulhu.Modules.Meta
{
    [Dependency ("CommandRootModule")]
    public class ModuleManagerModule : PluginBase {

        public override string Name => "Module Manager";
        public override string Description => "Manually keep track of and manage modules.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private ModuleManagerCommandSet moduleCommands = new ModuleManagerCommandSet ();

        public override void Initialize() {
            moduleCommands.ParentModule = this;
            ParentContainer.GetCommandRoot ().AddCommands (moduleCommands);
        }

        public override void Shutdown() {
            ParentContainer.GetCommandRoot ().RemoveCommands (moduleCommands);
        }

    }
}
