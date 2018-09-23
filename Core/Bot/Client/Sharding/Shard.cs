using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot.Client;
using Lomztein.Moduthulhu.Core.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class Shard {

        internal BotClient BotClient { get; private set; }
        internal ClientManager ClientManager { get => BotClient.ClientManager; }
        internal Core Core { get => BotClient.Core; }

        internal ModuleLoader ModuleLoader { get => BotClient.Core.ModuleLoader; }
        internal ModuleContainer ModuleContainer { get; private set; }

        public DiscordSocketClient Client { get; private set; }
        public IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }

        private Thread Thread { get; set; }

        public int ShardId { get; private set; }

        internal Shard (BotClient parentManager, int shardId) {
            BotClient = parentManager;
            ShardId = shardId;

            ThreadStart start = new ThreadStart (Initialize);
            Thread = new Thread (start);
        }

        private void Initialize () {
            DiscordSocketConfig config = new DiscordSocketConfig () {
                ShardId = ShardId,
                TotalShards = BotClient.TotalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            };
            Client = new DiscordSocketClient (config);
        }

    }
}
