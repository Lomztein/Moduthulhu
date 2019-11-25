using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class StatusMessage
    {
        private ActivityType _type;
        private Func<string> _message;

        public StatusMessage (ActivityType type, Func<string> message)
        {
            _type = type;
            _message = message;
        }

        public void ApplyTo (DiscordSocketClient client)
        {
            IActivity activity = new Game(_message(), _type);
            client.SetActivityAsync(activity);
        }
    }
}
