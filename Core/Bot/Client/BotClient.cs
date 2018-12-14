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
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotClient
    {
        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        //internal const int GUILDS_PER_SHARD = 2000;
        public ClientManager ClientManager { get; private set; }
        public Core Core { get => ClientManager.Core; }

        public string Name { get; private set; }
        internal string Token { get; set; }

        public string BaseDirectory { get => ClientManager.ClientsDirectory + "/" + Name + "/"; }

        internal Shard[] Shards { get; private set; }
        internal int ClientSlotIndex { get; private set; }
        public int TotalShards { get; private set; } = 1;
        public IEnumerable<SocketGuild> AllGuilds { get => Shards.SelectMany (x => x.Guilds); }
        public DiscordSocketClient FirstClient { get => Shards.First ().Client; }

        public event Action<Shard> ShardSpawned;
        public event Action<Shard> ShardKilled;
        public event Func<Exception, Task> ExceptionCaught;

        public UserList ClientAdministrators { get; private set; }

        internal BotClient (ClientManager clientManager, string name, int slotIndex) {

            BootDate = DateTime.Now;
            ClientManager = clientManager;
            Name = name;
            ClientSlotIndex = slotIndex;

            Token = File.ReadAllLines (BaseDirectory + "token.txt")[0];
            ClientAdministrators = new UserList (Path.Combine (BaseDirectory, "ClientAdministratorIDs"));

            Core.Clock.OnDayPassed += Clock_OnDayPassed;

            Shards = new Shard[TotalShards];

            Log.Write (Log.Type.BOT, "Creating bot client " + Name + " with token " + Token);
        }

        private Task Clock_OnDayPassed(DateTime currentTick, DateTime lastTick) {
            FirstClient.SetActivityAsync (new Game ($"on {AllGuilds.Count ()} servers with {AllGuilds.Sum (x => x.MemberCount)} users for  {(int)Uptime.TotalDays} days."));
            return Task.CompletedTask;
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
            ShardKilled?.Invoke (shard);
        }

        internal async Task RestartShard (Shard shard) {
            int id = shard.ShardId;
            await KillShard (shard);
            SpawnShard (id);
        }

        internal Shard SpawnShard (int shardId) {
            Shard shard = new Shard (this, shardId);
            shard.Begin ();
            ShardSpawned?.Invoke (shard);
            shard.ExceptionCaught += ExceptionCaught;
            shard.LoggedIn += Shard_ReadyAsync;
            return shard;
        }

        private async Task Shard_ReadyAsync() {
            await Task.WhenAll (Shards.Select (x => x.AwaitConnected ()).ToArray ());
            await Clock_OnDayPassed (DateTime.Now, DateTime.Now);
        }

        public override string ToString() => Name;

        public string GetStatusString() => $"Name: {Name} - Index: {ClientSlotIndex} - Shards: {Shards.Sum (x => x == null ? 0 : 1)} / {TotalShards}";
        public string GetShardsStatus() => Shards.Select (x => x == null ? "Dead shard; please restart client." : x.GetStatusString ()).Singlify ("\n");
    }
}
