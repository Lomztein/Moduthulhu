using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Cross;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotClient
    {
        //internal const int GUILDS_PER_SHARD = 2000;
        public ClientManager ClientManager { get; private set; }
        public Core Core { get => ClientManager.Core; }
        public TimeSpan Uptime { get => DateTime.Now - Core.BootDate; }

        public string Name { get; private set; }
        internal string Token { get; set; }

        public string BaseDirectory { get => ClientManager.ClientsDirectory + "/" + Name + "/"; }

        internal Shard[] Shards { get; private set; }
        public int TotalShards { get; private set; } = 1;
        public IEnumerable<SocketGuild> AllGuilds { get => Shards.SelectMany (x => x.Guilds); }
        public DiscordSocketClient FirstClient { get => Shards.First ().Client; }

        public event Action<Shard> OnShardSpawned;
        public event Action<Shard> OnShardKilled;
        public event Action<Exception> OnExceptionCaught;

        public UserList ClientAdministrators { get; private set; }

        internal BotClient (ClientManager clientManager, string name) {

            ClientManager = clientManager;

            Name = name;
            Token = File.ReadAllLines (BaseDirectory + "token.txt")[0];
            ClientAdministrators = new UserList (Path.Combine (BaseDirectory, "ClientAdministratorIDs"));

            Shards = new Shard[TotalShards];

            Log.Write (Log.Type.BOT, "Creating bot client " + Name + " with token " + Token);
        }

        internal void InitializeShards () {

            for (int i = 0; i < TotalShards; i++) {
                Shards[i] = SpawnShard (i);
            }

        }

        internal async Task Kill () {
            foreach (Shard shard in Shards) {
                await KillShard (shard);
            }
            Shards = new Shard[TotalShards];
        }

        internal async Task KillShard (Shard shard) {
            await shard.Kill ();
            OnShardKilled?.Invoke (shard);
        }

        internal async Task RestartShard (Shard shard) {
            int id = shard.ShardId;
            await KillShard (shard);
            SpawnShard (id);
        }

        internal Shard SpawnShard (int shardId) {
            Shard shard = new Shard (this, shardId);
            shard.Initialize ();
            OnShardSpawned?.Invoke (shard);
            shard.OnExceptionCaught += OnExceptionCaught;
            return shard;
        }

        public override string ToString() => Name;

    }
}
