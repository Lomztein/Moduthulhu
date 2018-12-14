using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice.Extensions
{
    public static class UserExtensions {

        public static SocketGuildUser IsInVoiceChannel(this IUser user) {
            if (user is SocketGuildUser guildUser) {

                if (guildUser.VoiceChannel != null) {
                    return guildUser;
                } else {
                    throw new InvalidOperationException($"**{user.GetShownName()}** is not currently in a voic channel.");
                }
            }

            throw new InvalidOperationException ("Something went terribly wrong, Ragnarok is coming.");
        }
    }
}
