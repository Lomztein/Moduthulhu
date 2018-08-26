using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using Lomztein.AdvDiscordCommands.Framework;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public class CustomCommandSet : ModuleCommandSet<CustomCommandsModule>, ICustomCommand {

        public CustomCommandSet () {
            AvailableOnServer = true;
            AvailableInDM = true;
            CommandEnabled = true;
        }

        public CommandAccessability Accessability { get; set; }
        public ulong OwnerID { get; set; }

        public override string AllowExecution(CommandMetadata data) {
            return this.CheckAccessability (base.AllowExecution (data), data.Message as SocketMessage);
        }

        public CustomCommandData SaveToData() {
            CustomSetData data = new CustomSetData ();
            data.SetBaseValues (this);

            foreach (var cmd in commandsInSet) {
                ICustomCommand customCmd = cmd as ICustomCommand;
                data.nestedCommands.Add (customCmd.SaveToData () as CustomChainData);
            }

            return data;
        }

    }
}
