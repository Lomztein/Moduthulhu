using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Dependency ("Lomztein-Command Root")]
    [Descriptor("Lomztein", "Administrator", "Implements commands for managing the bot itself.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    public class AdministrationPlugin : PluginBase {

        private CoreAdminCommands CoreCommands { get; set; }

        public override void Initialize() {
            CoreCommands = new CoreAdminCommands () { ParentPlugin = this };
            SendMessage("Lomztein-Command Root", "AddCommand", CoreCommands);
        }

        public override void Shutdown() {
            SendMessage("Lomztein-Command Root", "RemoveCommand", CoreCommands);
        }
    }
}
