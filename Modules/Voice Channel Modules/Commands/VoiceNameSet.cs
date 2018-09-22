using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.CustomCommands.Categories;
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
            Name = "voice";
            Description = "Command related to voice channels.";
            Category = AdditionalCategories.Voice;

            commandsInSet = new List<ICommand> () {
                new CustomName (),
            };
        }

        public class CustomName : ModuleCommand<AutoVoiceNameModule> {

            public CustomName () {
                Name = "name";
                Description = "Custom names for channel.";
                Category = AdditionalCategories.Voice;
            }

            [Overload (typeof (void), "Reset a custom channel name.")]
            public Task<Result> Execute (CommandMetadata data) {
                if (data.Message.Author.IsInVoiceChannel (out Task<Result> errorResult, out SocketGuildUser guildUser)) {
                    ParentModule.ResetCustomName (guildUser.VoiceChannel);
                    return TaskResult (null, "Succesfully reset custom voice channel name.");
                }
                return errorResult;
            }

            [Overload (typeof (void), "Reset a custom channel name.")]
            public Task<Result> Execute(CommandMetadata data, string name) {
                if (data.Message.Author.IsInVoiceChannel (out Task<Result> errorResult, out SocketGuildUser guildUser)) {
                    ParentModule.SetCustomName (guildUser.VoiceChannel, name);
                    return TaskResult (null, $"Succesfully set custom voice channel name to {name}.");
                }
                return errorResult;
            }
        }
    }
}
