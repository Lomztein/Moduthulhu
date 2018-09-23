using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot.Client;
using Lomztein.Moduthulhu.Core.Module;
using Lomztein.Moduthulhu.Cross;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        }

        internal void Begin () {
            ThreadStart start = new ThreadStart (Initialize);
            Thread = new Thread (start);
        }

        internal async void Initialize () {
            DiscordSocketConfig config = new DiscordSocketConfig () {
                ShardId = ShardId,
                TotalShards = BotClient.TotalShards,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            };
            Client = new DiscordSocketClient (config);

            await Start ();
            await Login ();

            Client.MessageReceived += Client_MessageReceived;
        }

        private Task Client_MessageReceived(SocketMessage arg) {
            Log.Write (Log.Type.CHAT, arg.Content);
            return Task.CompletedTask;
        }

        private async Task Start () {
            await Client.StartAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} started.");
        }

        private async Task Stop() {
            await Client.StopAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} stopped.");
        }

        private async Task Login () {
            await Client.LoginAsync (TokenType.Bot, BotClient.Token);
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} logged in.");
        }

        private async Task Logout () {
            await Client.LogoutAsync ();
            Log.Write (Log.Type.BOT, $"Client {BotClient.Name} shard {ShardId} logged out in.");
        }

        internal async Task Kill () {
            Log.Write (Log.Type.CRITICAL, $"KILLING CLIENT {BotClient.Name} SHARD {ShardId}!");
            await Logout ();
            await Stop ();
            Client.Dispose ();
        }

    }
}
