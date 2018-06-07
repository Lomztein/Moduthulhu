using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Module;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Lomztein.Moduthulhu.Core.Bot {

    /// <summary>
    /// A wrapper for the Discord.NET DiscordClient.
    /// </summary>
    public class BotClient : IDiscordClient {

        public DiscordSocketClient discordClient;
        private ModuleHandler moduleHandler;

        private string token;

        public string baseDirectory = AppContext.BaseDirectory;

        private DiscordSocketConfig socketConfig = new DiscordSocketConfig () {
            DefaultRetryMode = RetryMode.AlwaysRetry,
        };

        public async Task Run() {
            await ConnectAndInitialize ();
            await Task.Delay (-1);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }

        private async Task ConnectAndInitialize () {
            await Connect ();
            Initialize ();
        }

        private async Task Connect () {
            token = File.ReadAllText (baseDirectory + "/token.txt");

            Log.Write (Log.Type.BOT, "Initializing bot client!");
            discordClient = new DiscordSocketClient (socketConfig);
            Log.Write (Log.Type.BOT, "Logging in!");
            await discordClient.LoginAsync (TokenType.Bot, token);
            await discordClient.StartAsync ();
        }

        private void Initialize () {
            InitializeListeners ();
            moduleHandler = new ModuleHandler (this, baseDirectory + "/Modules/");
        }
        
        private void InitializeListeners () {
            discordClient.Disconnected += OnDisconnected;
            discordClient.JoinedGuild += OnJoinedGuild;
            discordClient.Connected += OnConnected;
            discordClient.Ready += OnReady;
            discordClient.Log += OnLog;
        }

        private Task OnJoinedGuild(SocketGuild arg) {
            moduleHandler.AutoConfigureModules (); // Reload configuration when joined a new server.
            return Task.CompletedTask;
        }

        private Task OnLog(LogMessage log) {
            Log.Write (Log.Type.BOT, log.Severity + " - " + log.Message);
            if (log.Exception != null)
                Log.Write (log.Exception);
            return Task.CompletedTask;
        }

        private Task OnConnected() {
            return Task.CompletedTask;
        }

        private async Task OnDisconnected(Exception arg) {
            Log.Write (arg);
            await discordClient.SetActivityAsync (new Game ("Last Disconnect: " + DateTime.Now.ToString (), ActivityType.Playing));
            //TryReconnect ();
        }

        private async Task Disconnect () {
            await discordClient.LogoutAsync ();
            discordClient.Dispose ();
            moduleHandler.ShutdownAllModules ();
            isReady = false;
        }

        private async void TryReconnect () {
            await Disconnect ();
            await ConnectAndInitialize ();
            await discordClient.SetActivityAsync (new Game ("Last Disconnect: " + DateTime.Now.ToString (), ActivityType.Playing));
        }

        private Task OnReady () {
            isReady = true;
            return Task.CompletedTask;
        }

        private bool isReady = false;

        public async Task AwaitFullBoot () {
            while (isReady == false)
                await Task.Delay (100);
            return;
        }

        public bool IsMultiserver () {
            if (!isReady)
                throw new InvalidOperationException ("Cannot call IsMultiserver before bot is fully booted and connected.");
            return discordClient.Guilds.Count != 1;
        }

        public SocketGuild GetGuild () {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a guild without ID from a multiserver bot.");
            return discordClient.Guilds.FirstOrDefault ();
        }

        public SocketGuild GetGuild (ulong id) {
            return discordClient.GetGuild (id);
        }

        public SocketGuildUser GetUser (ulong id) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a user without guild ID from a multiserver bot.");
            return GetGuild ()?.GetUser (id);
        }

        public SocketGuildUser GetUser(ulong guildID, ulong userID) {
            return GetGuild (guildID)?.GetUser (userID);
        }

        public SocketRole GetRole (ulong id) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a role without guild ID from a multiserver bot.");
            return GetGuild ()?.GetRole (id);
        }

        public SocketRole GetRole (ulong guildID, ulong roleID) {
            return GetGuild (guildID)?.GetRole (roleID);
        }

        public SocketGuildChannel GetChannel (ulong id) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a channel without guild ID from a multiserver bot.");
            return GetGuild ()?.GetChannel (id);
        }

        public SocketGuildChannel GetChannel (ulong guildID, ulong channelID) {
            return GetGuild (guildID)?.GetChannel (channelID);
        }

        public async Task<IMessage> GetMessage (ulong channelID, ulong messageID) {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a message without guild ID from a multiserver bot.");
            return await (GetGuild ()?.GetChannel (channelID) as SocketTextChannel)?.GetMessageAsync (messageID);
        }

        public async Task<IMessage> GetMessage(ulong guildID, ulong channelID, ulong messageID) {
            return await (GetGuild (guildID)?.GetChannel (channelID) as SocketTextChannel)?.GetMessageAsync (messageID);
        }

        // Implement wrappers for the internal discord client.
        public ConnectionState ConnectionState => ((IDiscordClient)discordClient).ConnectionState;

        public ISelfUser CurrentUser => ((IDiscordClient)discordClient).CurrentUser;

        public TokenType TokenType => ((IDiscordClient)discordClient).TokenType;

        public Task StartAsync() {
            return ((IDiscordClient)discordClient).StartAsync ();
        }

        public Task StopAsync() {
            return ((IDiscordClient)discordClient).StopAsync ();
        }

        public Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetApplicationInfoAsync (options);
        }

        public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetChannelAsync (id, mode, options);
        }

        public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetPrivateChannelsAsync (mode, options);
        }

        public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetDMChannelsAsync (mode, options);
        }

        public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetGroupChannelsAsync (mode, options);
        }

        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetConnectionsAsync (options);
        }

        public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetGuildAsync (id, mode, options);
        }

        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetGuildsAsync (mode, options);
        }

        public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).CreateGuildAsync (name, region, jpegIcon, options);
        }

        public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetInviteAsync (inviteId, options);
        }

        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetUserAsync (id, mode, options);
        }

        public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetUserAsync (username, discriminator, options);
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetVoiceRegionsAsync (options);
        }

        public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetVoiceRegionAsync (id, options);
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null) {
            return ((IDiscordClient)discordClient).GetWebhookAsync (id, options);
        }

        public void Dispose() {
            ((IDiscordClient)discordClient).Dispose ();
        }
        // Wrappers' done ya'll.
    }
}
