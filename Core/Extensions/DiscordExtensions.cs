using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace Lomztein.Moduthulhu.Core.Extensions
{
    /// <summary>
    /// Contains a bunch of utility extension methods for various Discord objects.
    /// </summary>
    public static class DiscordExtensions
    {
        public static string GetPath (this IChannel channel) {

            if (channel == null)
                return "null";

            if (channel is SocketGuildChannel guildChannel) {
                if (guildChannel.Category != null)
                    return $"{guildChannel.Guild.Name} / {guildChannel.Category.Name} / {guildChannel.Name}";
                else
                    return $"{guildChannel.Guild.Name} / {guildChannel.Name}";
            }

            if (channel is SocketDMChannel dmChannel)
                return $"{dmChannel.Recipient.Username}";

            return "";
        }

        public static string GetPath (this IMessage message) {
            if (message is SocketUserMessage userMessage)
                return userMessage.Channel.GetPath () + " / " + userMessage.Author.Username;
            if (message is SocketSystemMessage systemMessage)
                return systemMessage.Source.ToString ();
            return "";
        }
        
        public static string GetPath (this SocketGuildUser guildUser) {
            if (guildUser == null)
                return "null";

            return guildUser.Guild.Name + " / " + (string.IsNullOrEmpty (guildUser.Nickname) ? guildUser.Username : guildUser.Nickname);
        }

        public static string GetPath (this IRole role) {

            if (role == null)
                return "null";

            return role.Guild + " / " + role.Name;

        }
    }
}
