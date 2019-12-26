using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Dependency ("Moduthulhu-Command Root")]
    [Descriptor("Moduthulhu", "Administrator", "Implements commands for managing the bot itself.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Core/Plugin/Standard%20Plugins/Administrator")]
    public class AdministrationPlugin : PluginBase {

        private CoreAdminCommands CoreCommands { get; set; }

        public override void Initialize() {
            CoreCommands = new CoreAdminCommands { ParentPlugin = this };
            SendMessage("Moduthulhu-Command Root", "AddCommand", CoreCommands);
        }

        public override void Shutdown() {
            SendMessage("Moduthulhu-Command Root", "RemoveCommand", CoreCommands);
        }
    }
}
