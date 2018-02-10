using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.ModularDiscordBot.Core.Module;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.ExampleCommands;
using System.IO;
using System.Threading;

namespace Lomztein.ModularDiscordBot.Core.Bot {

    /// <summary>
    /// A wrapper for the Discord.NET DiscordClient.
    /// </summary>
    public class BotClient {

        public DiscordSocketClient discordClient;
        private ModuleHandler moduleHandler;
        private CommandRoot commandRoot;

        private CancellationToken shutdownToken = new CancellationToken ();

        private string token;

        public string baseDirectory = AppContext.BaseDirectory;

        public async Task Initialize() {

            token = File.ReadAllText (baseDirectory + "/token.txt");

            Log.Write (Log.Type.BOT, "Initializing bot client!");
            discordClient = new DiscordSocketClient ();
            Log.Write (Log.Type.BOT, "Logging in!");
            await discordClient.LoginAsync (TokenType.Bot, token);
            await discordClient.StartAsync ();

            InitializeListeners ();
            InitializeCommandRoot ();

            moduleHandler = new ModuleHandler (this, baseDirectory + "/Modules/");

            await Task.Delay (-1, shutdownToken);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }
        
        private void InitializeListeners () {
            discordClient.Disconnected += OnDisconnected;
            discordClient.Connected += OnConnected;
        }

        private Task OnConnected() {
            Log.Write (Log.Type.BOT, "Connected to Discord!");
            return Task.CompletedTask;
        }

        private async Task OnDisconnected(Exception arg) {
            Log.Write (arg);
            while (discordClient.ConnectionState == ConnectionState.Disconnected) {
                await discordClient.LoginAsync (TokenType.Bot, token);
                await Task.Delay (1000 * 60 * 5);
            }
        }

        private void InitializeCommandRoot () {
            commandRoot = new CommandRoot ();
        }

        public CommandRoot GetCommandRoot () {
            return commandRoot;
        }
    }
}
