using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class StatusMessage : IActivity
    {
        public ActivityType Type { get; }
        private readonly Func<string> _message;
        public string Name => _message();

        public StatusMessage (ActivityType type, Func<string> message)
        {
            Type = type;
            _message = message;
        }

    }
}
