using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.ModularDiscordBot.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Karma.Commands
{
    public class KarmaCommand : ModuleCommand<KarmaModule> {

        public KarmaCommand() {
            command = "karma";
            shortHelp = "Shows karma.";
        }

        [Overload (typeof (int), "Returns your own karma.")]
        public Task<Result> Execute (CommandMetadata data) {
            return Execute (data, data.message.Author);
        }

        [Overload (typeof (int), "Returns karma of a given user.")]
        public Task<Result> Execute (CommandMetadata data, SocketUser user) {
            int karma = parentModule.GetKarma (user.Id);
            return TaskResult (karma, $"User {user.GetShownName ()} has {karma} karma!");
        }
    }
}
