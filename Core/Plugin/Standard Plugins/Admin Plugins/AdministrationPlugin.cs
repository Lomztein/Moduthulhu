using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Descriptor("Lomztein", "Administrator Plugin", "Implements commands for managing the bot itself.")]
    [Source("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu")]
    public class AdministrationPlugin : PluginBase {

        private CoreAdminCommands CoreCommands { get; set; }

        public override void Initialize() {
            CoreCommands = new CoreAdminCommands () { ParentPlugin = this };
            SendMessage("CommandRoot.AddCommand", CoreCommands);
        }

        public override void Shutdown() {
            SendMessage("CommandRoot.RemoveCommand", CoreCommands);
        }
    }
}
