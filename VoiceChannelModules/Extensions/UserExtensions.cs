using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice.Extensions
{
    public static class UserExtensions {

        public static bool IsInVoiceChannel(this IUser user, out Task<Result> result, out SocketGuildUser gUser) {
            if (user is SocketGuildUser guildUser) {
                gUser = guildUser;

                if (guildUser.VoiceChannel != null) {
                    result = null;
                    return true;
                } else {
                    result = Task.FromResult(new Result (null, "Error - You're not in a voice channel."));
                    return false;
                }
            }

            result = Task.FromResult (new Result (null, "Error - Something went terribly wrong, Ragnarok is coming."));
            gUser = null;

            return false;
        }
    }
}
