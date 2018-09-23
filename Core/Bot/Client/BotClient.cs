using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Cross;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotClient
    {
        internal ClientManager ClientManager { get; private set; }
        internal Core Core { get => ClientManager.Core; }
        public TimeSpan Uptime { get => DateTime.Now - Core.BootDate; }

        public string Name { get; private set; }
        private string Token { get; set; }

        internal List<Shard> ActiveShards { get; private set; }
        public int ActiveShardsCount { get => ActiveShards.Count; }
        public int TotalShards { get; private set; }

        internal BotClient (ClientManager clientManager, string token, string name) {
            Log.Write (Log.Type.BOT, "Creating bot client " + name + " with token " + Token);
            ClientManager = clientManager;
            Token = token;
            Name = name;
        }

        internal void Kill () {
            throw new NotImplementedException ();
        }

        internal Shard SpawnShard (int shardId) {

            if (ActiveShards.Count == TotalShards - 1)
                throw new InvalidOperationException ("Spawning another shard would exceed the shard amount of this client.");

            Shard shard = new Shard (this, shardId);
            ActiveShards.Add (shard);
            return shard;
        }

    }
}
