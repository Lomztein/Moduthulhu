using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot
{
    public class ShardedClientWrapper : IClientWrapper
    {
        public DiscordShardedClient Client;

        public Task StartAsync () {
            Client.Di
        }

        public Task LoginAsync () {

        }

        public Task DisconnectAsync () {

        }
    }
}
