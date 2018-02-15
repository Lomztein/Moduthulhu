using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.ModularDiscordBot.Core.Module;
using System.IO;
using System.Threading;
using System.Linq;

namespace Lomztein.ModularDiscordBot.Core.Bot {

    /// <summary>
    /// A wrapper for the Discord.NET DiscordClient.
    /// </summary>
    public class BotClient {

        public DiscordSocketClient discordClient;
        private ModuleHandler moduleHandler;

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

            moduleHandler = new ModuleHandler (this, baseDirectory + "/Modules/");

            await Task.Delay (-1);
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

        private bool IsBooted() => discordClient.Guilds.Count > 0 && discordClient.Guilds.ElementAtOrDefault (0) != null && discordClient.Guilds.ElementAt (0).Users.ElementAtOrDefault (0) != null;

        public async Task AwaitFullBoot () {
            while (IsBooted () == false)
                await Task.Delay (100);
            return;
        }

        public bool IsMultiserver () {
            if (!IsBooted ())
                throw new InvalidOperationException ("Cannot call IsMultiserver before bot is fully booted and connected.");
            return discordClient.Guilds.Count != 1;
        }

        public SocketGuild GetGuild () {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a guild without ID from a multiserver bot.");
            return discordClient.Guilds.First ();
        }

        public SocketGuild GetGuild (ulong id) {
            return discordClient.GetGuild (id);
        }

        public SocketGuildUser GetUser (ulong id) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a user without guild ID from a multiserver bot.");
            return GetGuild ().GetUser (id);
        }

        public SocketGuildUser GetUser(ulong guildID, ulong userID) {
            return GetGuild (guildID).GetUser (userID);
        }

        public SocketRole GetRole (ulong id) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a role without guild ID from a multiserver bot.");
            return GetGuild ().GetRole (id);
        }

        public SocketRole GetRole (ulong guildID, ulong roleID) {
            return GetGuild (guildID).GetRole (roleID);
        }

        public SocketGuildChannel GetChannel (ulong id) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a channel without guild ID from a multiserver bot.");
            return GetGuild ().GetChannel (id);
        }

        public SocketGuildChannel GetChannel (ulong guildID, ulong channelID) {
            return GetGuild (guildID)?.GetChannel (channelID);
        }

        public async Task<IMessage> GetMessage (ulong channelID, ulong messageID) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a message without guild ID from a multiserver bot.");
            return await (GetGuild ().GetChannel (channelID) as SocketTextChannel)?.GetMessageAsync (messageID);
        }

        public async Task<IMessage> GetMessage(ulong guildID, ulong channelID, ulong messageID) {
            return await (GetGuild (guildID).GetChannel (channelID) as SocketTextChannel)?.GetMessageAsync (messageID);
        }
    }
}
