using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
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
        public Core Core { get; private set; }

        private ClientConfiguration _configuration;

        public string DataDirectory { get => Core.DataDirectory; }

        private Shard[] _shards;
        private IEnumerable<SocketGuild> AllGuilds { get => _shards.SelectMany (x => x.Guilds); }
        private DiscordSocketClient FirstClient { get => _shards.First ().Client; }

        private readonly Clock _statusClock = new Clock(1, "StatusClock");

        public event Func<Exception, Task> ExceptionCaught;

        internal BotClient (Core core) {

            BootDate = DateTime.Now;
            Core = core;

            _configuration = LoadConfiguration(DataDirectory + "Configuration");

            Log.Write (Log.Type.BOT, "Creating bot client with token " + _configuration.Token);
        }

        private ClientConfiguration LoadConfiguration (string path)
        {
            Log.Write(Log.Type.BOT, "Loading configuration for bot client.");
            _configuration = ClientConfiguration.Load(path);
            if (_configuration == null)
            {
                // If no file exists, create a new one.
                _configuration = new ClientConfiguration();
                _configuration.Save(path);
            }

            _configuration.CheckValidity();
            
            return _configuration;
        }

        private Task UpdateStatus (DateTime currentTick, DateTime lastTick) {
            FirstClient.SetActivityAsync (new Game ($"on {AllGuilds.Count ()} servers with {AllGuilds.Sum (x => x.MemberCount)} users for  {(int)Uptime.TotalDays} days."));
            return Task.CompletedTask;
        }

        public async void Initialize ()
        {
            InitializeShards();
            await AwaitAllConnected();
            await UpdateStatus(DateTime.Now, DateTime.Now);

            _statusClock.OnDayPassed += UpdateStatus;
            _statusClock.Start();
        }

        internal void InitializeShards () {
            _shards = new Shard[_configuration.TotalShards];
            for (int i = _configuration.ShardRange.Min; i < _configuration.ShardRange.Max; i++) {
                _shards[i] = CreateShard (i, _configuration.TotalShards);
                _shards[i].Run();
            }
        }

        internal Shard CreateShard (int shardId, int totalShards) {
            Shard shard = new Shard (this, _configuration.Token, shardId, totalShards);
            shard.ExceptionCaught += OnExceptionCaught;
            return shard;
        }

        private Task OnExceptionCaught (Exception exception)
        {
            FirstClient.SetGameAsync(exception.Message + " from " + exception.Source + " in " + exception.TargetSite);
            return ExceptionCaught?.Invoke(exception);
        }

        private async Task AwaitAllConnected () {
            await Task.WhenAll (_shards.Select (x => x.AwaitConnected ()).ToArray ());
        }

        public string GetStatusString() => $"Shards: {_shards.Sum (x => x == null ? 0 : 1)} / {_configuration.TotalShards}";
        public string GetShardsStatus() => _shards.Select (x => x == null ? "Dead shard; please restart client." : x.GetStatusString ()).Singlify ("\n");
    }
}
