using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.Administration.AdministrationCommands;
using Lomztein.Moduthulhu.Modules.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Administration
{
    [Dependency ("CommandRootModule")]
    public class AdministrationModule : ModuleBase {

        public override string Name => "Administration Module";
        public override string Description => "Module for administrating the core and client processes.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private CoreAdminCommands CoreCommands { get; set; }
        private ClientAdminCommands ClientCommands { get; set; }

        public override void Initialize() {
            CoreCommands = new CoreAdminCommands () { ParentModule = this };
            ClientCommands = new ClientAdminCommands () { ParentModule = this };
            ParentContainer.GetCommandRoot ().AddCommands (CoreCommands, ClientCommands);
        }

        public override void Shutdown() {
            ParentContainer.GetCommandRoot ().RemoveCommands (CoreCommands, ClientCommands);
        }
    }
}
