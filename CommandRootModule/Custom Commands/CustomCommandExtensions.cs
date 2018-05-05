using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public static class CustomCommandExtensions
    {
        public static string CheckAccessability (this ICustomCommand customCommand, string baseErrors, SocketMessage e) {

            switch (customCommand.Accessability) {

                case CommandAccessability.Private:
                    if (e.Author.Id != customCommand.OwnerID)
                        baseErrors += "\n\tOnly available to author.";
                    break;

                case CommandAccessability.Public:
                    if (e.Channel is SocketGuildChannel guildChannel) {
                        if (guildChannel.Guild.Id != customCommand.OwnerID)
                            baseErrors += "\n\tOnly available on authors server.";
                    } else {
                        baseErrors += "\n\tOnly available on authors server.";
                    }
                    break;
            }

            return baseErrors;

        }

        public static void SetBaseValues(this CustomCommandData commandData, ICustomCommand command) {
            commandData.accessability = command.Accessability;
            commandData.ownerID = command.OwnerID;
            commandData.name = command.Name;

            if (command is Command cmd) {
                commandData.catagory = cmd.catagory;
            }
        }

        public static bool ContainsCommandByName (this ICommandSet commandSet, string name) {
            return commandSet.GetCommands ().Exists (x => x.command == name);
        }
    }
}
