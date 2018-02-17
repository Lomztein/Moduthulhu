using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Core.Bot
{
    public static class MessageControl
    {
        public static async Task<IUserMessage> SendMessage(ITextChannel channel, string text, bool isTTS = false, Embed embed = null) {
            if (text == null)
                text = "";
            return await channel.SendMessageAsync (text, isTTS, embed);
        }

    }
}
