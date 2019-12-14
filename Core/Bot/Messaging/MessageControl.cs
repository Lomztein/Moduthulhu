using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;

namespace Lomztein.Moduthulhu.Core.Bot.Messaging
{
    public static class MessageControl
    {
        public static async Task<IUserMessage> SendMessage(ITextChannel channel, string text, bool isTTS, Embed embed) {
            
            if (string.IsNullOrWhiteSpace (text) && embed == null) {
                return null;
            }

            return await channel.SendMessageAsync (text == null ? string.Empty : text, isTTS, embed);
        }

        public static async Task<IUserMessage> SendMessage(ITextChannel channel, string text, bool isTTS) => await SendMessage(channel, text, isTTS, null);
        public static async Task<IUserMessage> SendMessage(ITextChannel channel, string text) => await SendMessage(channel, text, false, null);
    }
}
