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
using Lomztein.Moduthulhu.Core.Plugins;

namespace Lomztein.Moduthulhu.Core.Bot.Client
{
    public class BotClient
    {
        public DateTime BootDate { get; private set; }
        public TimeSpan Uptime { get => DateTime.Now - BootDate; }

        //internal const int GUILDS_PER_SHARD = 2000;
        public BotCore Core { get; private set; }

        private ClientConfiguration _configuration;

        public static string DataDirectory { get => BotCore.DataDirectory; }

        private BotShard[] _shards;
        private IEnumerable<SocketGuild> AllGuilds { get => _shards.SelectMany (x => x.Guilds); }
        private DiscordSocketClient FirstClient { get => _shards.First ().Client; }

        private Dictionary<string, StatusMessage> _statusMessages = new Dictionary<string, StatusMessage>();
        private int _statusMessageIndex = -1;
        private const int _statusChangeChance = 10;

        private readonly Clock _statusClock = new Clock(1, "StatusClock");
        private UserList _botAdministrators;
        private int _consecutiveOfflineMinutes = 0;
        private int _automaticOfflineMinutesTreshold = 10;

        public event Func<Exception, Task> ExceptionCaught;

        internal BotClient (BotCore core) {

            BootDate = DateTime.Now;
            Core = core;

            _configuration = LoadConfiguration(DataDirectory + "/Configuration");

            Log.Write (Log.Type.BOT, "Creating bot client with token " + _configuration.Token);
        }

        internal async void Initialize()
        {
            PluginLoader.ReloadPluginAssemblies();
            _botAdministrators = new UserList(Path.Combine(DataDirectory, "/Administrators"));

            InitializeShards();
            await AwaitAllConnected();

            InitStatusMessages();
            _statusClock.OnMinutePassed += StatusClock_OnMinutePassed;
            _statusClock.OnMinutePassed += RotateStatus;
            _statusClock.Start();

            _statusMessages.First().Value.ApplyTo(FirstClient);
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

        private Task RotateStatus(DateTime previous, DateTime now)
        {
            if (new Random().Next(0, _statusChangeChance) == 0)
            {
                _statusMessageIndex++;
                _statusMessageIndex %= _statusMessages.Count;
                _statusMessages.ElementAt(_statusMessageIndex).Value.ApplyTo(FirstClient);
            }
            return Task.CompletedTask;
        }

        public void AddStatusMessage (string identifier, ActivityType activityType, Func<string> message)
        {
            if (_statusMessages.ContainsKey (identifier))
            {
                throw new ArgumentException($"Cannot add message with identifier '{identifier}' since one such already exists.");
            }
            else
            {
                _statusMessages.Add(identifier, new StatusMessage(activityType, message));
            }
        }

        public void RemoveStatusMessage(string identifier) => _statusMessages.Remove(identifier);



        private void InitStatusMessages ()
        {
            AddStatusMessage("UsersServed", ActivityType.Watching, () => new Random().Next(0, 100) == 0 ? $"{AllGuilds.SelectMany(x => x.Users).Count()} puny mortals waste away their hilariously short lives." : $"{AllGuilds.SelectMany(x => x.Users).Count()} users in {AllGuilds.Count()} servers.");
            AddStatusMessage("Help", ActivityType.Listening, () => new Random().Next(0, 100) == 0 ? "the sweet cries of the fresh virgin sacrifices." : "!help commands! Prefix may vary between servers.");
            AddStatusMessage("AvailablePlugins", ActivityType.Playing, () => new Random().Next(0, 100) == 0 ? "the dice of the vast cosmos." : $"with the {PluginLoader.GetPlugins().Length} plugins available. Try '!plugins ?'!");
            AddStatusMessage("Uptime", ActivityType.Streaming, () => new Random().Next(0, 100) == 0 ? "V̌̾͒̓͏̸̼͔̘͎̳̦̮̰̹̥Ǫ̪͎̜̝͙̅ͫ͊̃͗̾̍ͣ̔̾͊͆ͭ͗̏͆̀͘͟͠I̴͛͌ͦ͊̇̾ͮ͂̈̌͏̪̜̳͙̰̝̺̱͈̗̥D̡̳͈̠͔̲̳̤̱͚̤ͥͮͤͪ̄ͤ͐̆̿ͩ͐ͭ̋̂͗̔ͬͦ͊" : $"for {Uptime.Days} days of uninterrupted service!");
        }

        private Task StatusClock_OnMinutePassed(DateTime currentTick, DateTime lastTick)
        {
            if (_shards.Any (x => x.IsConnected == false)) {
                _consecutiveOfflineMinutes++;
                Log.Write(Log.Type.WARNING, $"Disconnected shard detected, commencing auto-shutdown in {_consecutiveOfflineMinutes}/{_automaticOfflineMinutesTreshold} minutes..");
            }
            else
            {
                if (_consecutiveOfflineMinutes > 0)
                {
                    Log.Write(Log.Type.CONFIRM, $"All connections reestablished, auto-shutdown cancelled.");
                }

                _consecutiveOfflineMinutes = 0;
            }

            if (_consecutiveOfflineMinutes >= _automaticOfflineMinutesTreshold)
            {
                Log.Write(Log.Type.CRITICAL, $"Commencing automatic shutdown due to disconnected shard. :(");
                Shutdown();
            }
            return Task.CompletedTask;
        }

        private void Shutdown()
        {
            Core.Shutdown();
        }

        internal void InitializeShards () {
            _shards = new BotShard[_configuration.TotalShards];
            for (int i = _configuration.ShardRange.Min; i < _configuration.ShardRange.Max; i++) {
                _shards[i] = CreateShard (i, _configuration.TotalShards);
                _shards[i].Run();
            }
        }

        public bool IsBotAdministrator(ulong userId)
        {
            return _botAdministrators.Contains(userId);
        }

        internal BotShard CreateShard (int shardId, int totalShards) {
            BotShard shard = new BotShard (this, _configuration.Token, shardId, totalShards);
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
