using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class StatusMessage
    {
        public readonly ActivityType Type;
        public readonly Func<string> Message;

        public StatusMessage (ActivityType type, Func<string> message)
        {
            Type = type;
            Message = message;
        }
    }
}
