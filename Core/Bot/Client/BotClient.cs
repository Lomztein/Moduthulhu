using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Cross;
using System.IO;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotClient
    {
        //internal const int GUILDS_PER_SHARD = 2000;
        internal ClientManager ClientManager { get; private set; }
        internal Core Core { get => ClientManager.Core; }
        public TimeSpan Uptime { get => DateTime.Now - Core.BootDate; }

        public string Name { get; private set; }
        internal string Token { get; set; }

        public string BaseDirectory { get => ClientManager.ClientsDirectory + "\\" + Name + "\\"; }

        internal Shard[] Shards { get; private set; }
        public int TotalShards { get; private set; } = 1;

        internal BotClient (ClientManager clientManager, string name) {

            ClientManager = clientManager;

            Name = name;
            Token = File.ReadAllLines (BaseDirectory + "token.txt")[0];

            Shards = new Shard[TotalShards];

            Log.Write (Log.Type.BOT, "Creating bot client " + Name + " with token " + Token);
        }

        internal void InitializeShards () {

            for (int i = 0; i < TotalShards; i++) {
                Shards[i] = SpawnShard (i);
            }

            foreach (Shard shard in Shards) {
                shard.Initialize ();
            }    

        }

        internal async Task Kill () {
            foreach (Shard shard in Shards) {
                await shard.Kill ();
            }
        }

        internal Shard SpawnShard (int shardId) {
            Shard shard = new Shard (this, shardId);
            return shard;
        }

        public override string ToString() => Name;

    }
}
