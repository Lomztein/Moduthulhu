using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public class CustomCommandSet : ModuleCommandSet<CustomCommandsModule>, ICustomCommand {

        public CustomCommandSet () {
            availableOnServer = true;
            availableInDM = true;
            commandEnabled = true;
        }

        public CommandAccessability Accessability { get; set; }
        public ulong OwnerID { get; set; }
        public string Name { get => command; }

        public override string AllowExecution(IMessage e) {
            return this.CheckAccessability (base.AllowExecution (e), e as SocketMessage);
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
