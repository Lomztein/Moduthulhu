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
            Description = "Voice related commands.";
            Category = AdditionalCategories.Voice;
            Flatname = "vname";

            commandsInSet = new List<ICommand> () {
                new CustomName (),
            };
        }

        public class CustomName : ModuleCommand<AutoVoiceNameModule> {

            public CustomName () {
                Name = "name";
                Description = "Specify channel name.";
                Category = AdditionalCategories.Voice;
                Flatname = "vname";
            }

            [Overload (typeof (void), "Reset a custom channel name.")]
            public async Task<Result> Execute (CommandMetadata data) {
                SocketGuildUser user = data.Message.Author.IsInVoiceChannel();
                if (user != null) {
                    await ParentModule.ResetCustomName (user.VoiceChannel);
                    return new Result (null, "Succesfully reset custom voice channel name.");
                }
                return new Result(null, "You aren't in a voice channel at the moment. At least not on this server.");
            }

            [Overload (typeof (void), "Set a custom channel name.")]
            public async Task<Result> Execute(CommandMetadata data, string name) {
                SocketGuildUser guildUser = data.Message.Author.IsInVoiceChannel();
                if (guildUser != null) {
                    await ParentModule.SetCustomName (guildUser.VoiceChannel, name);
                    return new Result(null, $"Succesfully set custom voice channel name to {name}.");
                }
                return new Result(null, "You aren't in a voice channel at the moment. At least not on this server.");
            }
        }
    }
}
