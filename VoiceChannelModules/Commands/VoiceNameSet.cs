using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.Voice.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice.Commands
{
    public class VoiceNameSet : ModuleCommandSet<AutoVoiceNameModule>
    {
        public VoiceNameSet () {
            command = "voice";
            shortHelp = "Command related to voice channels.";
            catagory = Category.Utility;

            commandsInSet = new List<Command> () {
                new CustomName (),
            };
        }

        public class CustomName : ModuleCommand<AutoVoiceNameModule> {

            public CustomName () {
                command = "name";
                shortHelp = "Custom names for channel.";
                catagory = Category.Utility;
            }

            [Overload (typeof (void), "Reset a custom channel name.")]
            public Task<Result> Execute (CommandMetadata data) {
                if (data.message.Author.IsInVoiceChannel (out Task<Result> errorResult, out SocketGuildUser guildUser)) {
                    ParentModule.ResetCustomName (guildUser.VoiceChannel);
                    return TaskResult (null, "Succesfully reset custom voice channel name.");
                }
                return errorResult;
            }

            [Overload (typeof (void), "Reset a custom channel name.")]
            public Task<Result> Execute(CommandMetadata data, string name) {
                if (data.message.Author.IsInVoiceChannel (out Task<Result> errorResult, out SocketGuildUser guildUser)) {
                    ParentModule.SetCustomName (guildUser.VoiceChannel, name);
                    return TaskResult (null, $"Succesfully set custom voice channel name to {name}.");
                }
                return errorResult;
            }
        }
    }
}
