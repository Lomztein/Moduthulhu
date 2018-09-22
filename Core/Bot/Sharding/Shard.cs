using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Sharding
{
    public class Shard
    {
        public ShardManager ParentManager { get; private set; }
        public Core Core { get => ParentManager.Core; }
        public ModuleLoader ModuleLoader { get => ParentManager.Core.ModuleLoader; }

        public ModuleContainer ModuleContainer { get; private set; }

        public DiscordSocketClient Client { get; private set; }

        public int ShardId { get => Client.ShardId; }

        public Shard (ShardManager parentManager, int shardId) {
            ParentManager = parentManager;

            DiscordSocketConfig config = new DiscordSocketConfig () {
                ShardId = shardId,
                TotalShards = ParentManager.TotalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            }
            Client = new DiscordSocketClient (config);
        }
    }
}
