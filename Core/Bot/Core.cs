using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Module;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Cross;

namespace Lomztein.Moduthulhu.Core.Bot {

    /// <summary>
    /// A wrapper for the Discord.NET DiscordClient.
    /// </summary>
    public class Core : IDiscordClient {

        // TODO: Split BotClient into a Core class and a IClientWrapper interface.
        // TODO: Move the Clock systems into the main Core area.
        // TODO: Allow for on-the-fly changing of avatar/username
        // TODO: Allow for sending bot-wide messages directly in console.

        public TimeSpan Uptime { get => DateTime.Now - BootDate; }
        public DateTime BootDate { get; private set; }

        public DiscordSocketClient DiscordClient { get; private set; }
        public ModuleLoader ModuleLoader { get; private set; }

        private string Token { get => File.ReadAllLines (BaseDirectory + "/token.txt")[0]; }

        public string BaseDirectory { get => AppContext.BaseDirectory; }
        public string AvatarPath { get => BaseDirectory + "avatar.png"; }
        public string UsernamePath { get => BaseDirectory + "username.txt"; }

        private DiscordSocketConfig socketConfig = new DiscordSocketConfig () {
            DefaultRetryMode = RetryMode.AlwaysRetry,
        };

        public async Task Run() {
            BootDate = DateTime.Now;
            await ConnectAndInitialize ();
            await Task.Delay (-1);
            Log.Write (Log.Type.BOT, "Shutting down..");
        }

        private async Task ConnectAndInitialize () {
            await Connect ();
            Initialize ();
        }

        private async Task Connect () {
            Log.Write (Log.Type.BOT, "Initializing bot client!");
            DiscordClient = new DiscordSocketClient (socketConfig);
            Log.Write (Log.Type.BOT, "Logging in!");
            await DiscordClient.LoginAsync (TokenType.Bot, Token);
            await DiscordClient.StartAsync ();
        }

        private void Initialize () {
            InitializeListeners ();
            Status.Set ("CorePath", BaseDirectory);
            ModuleLoader = new ModuleLoader (this, BaseDirectory + "/Modules/");
        }
        
        private void InitializeListeners () {
            DiscordClient.Disconnected += OnDisconnected;
            DiscordClient.JoinedGuild += OnJoinedGuild;
            DiscordClient.Connected += OnConnected;
            DiscordClient.Ready += OnReady;
            DiscordClient.Log += OnLog;
        }

        private Task OnJoinedGuild(SocketGuild arg) {
            ModuleLoader.AutoConfigureModules (); // Reload configuration when joined a new server.
            return Task.CompletedTask;
        }

        private void ModifyUserOnLaunch () {
            if (File.Exists (AvatarPath))
                SetAvatar (AvatarPath);
            if (File.Exists (UsernamePath))
                SetUsername (File.ReadAllLines (UsernamePath)[0]);
        }

        public void SetAvatar (string filePath) {
            Image image = new Image (filePath);
            DiscordClient.CurrentUser.ModifyAsync (x => x.Avatar = image );
        }

        public void SetUsername (string username) {
            DiscordClient.CurrentUser.ModifyAsync (x => x.Username = username);
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

        private Task OnDisconnected(Exception arg) {
            Log.Write (arg);
            UpdateUptimeDisplay ();
            //TryReconnect ();
            return Task.CompletedTask;
        }

        public void UpdateUptimeDisplay () {
            DiscordClient.SetGameAsync ("Current uptime: " + Math.Floor (Uptime.TotalDays) + " days.");
        }

        private async Task Disconnect () {
            await DiscordClient.LogoutAsync ();
            DiscordClient.Dispose ();
            ModuleLoader.ShutdownAllModules ();
            isReady = false;
        }

        private Task OnReady () {
            isReady = true;
            UpdateUptimeDisplay ();
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
            return DiscordClient.Guilds.Count != 1;
        }

        public SocketGuild GetGuild () {
            if (IsMultiserver ())
                throw new InvalidOperationException ("You shouldn't request a guild without ID from a multiserver bot.");
            return DiscordClient.Guilds.FirstOrDefault ();
        }

        public SocketGuild GetGuild (ulong id) {
            return DiscordClient.GetGuild (id);
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
        public ConnectionState ConnectionState => ((IDiscordClient)DiscordClient).ConnectionState;

        public ISelfUser CurrentUser => ((IDiscordClient)DiscordClient).CurrentUser;

        public TokenType TokenType => ((IDiscordClient)DiscordClient).TokenType;

        public Task StartAsync() {
            return ((IDiscordClient)DiscordClient).StartAsync ();
        }

        public Task StopAsync() {
            return ((IDiscordClient)DiscordClient).StopAsync ();
        }

        public Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetApplicationInfoAsync (options);
        }

        public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetChannelAsync (id, mode, options);
        }

        public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetPrivateChannelsAsync (mode, options);
        }

        public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetDMChannelsAsync (mode, options);
        }

        public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetGroupChannelsAsync (mode, options);
        }

        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetConnectionsAsync (options);
        }

        public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetGuildAsync (id, mode, options);
        }

        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetGuildsAsync (mode, options);
        }

        public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).CreateGuildAsync (name, region, jpegIcon, options);
        }

        public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetInviteAsync (inviteId, options);
        }

        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetUserAsync (id, mode, options);
        }

        public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetUserAsync (username, discriminator, options);
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetVoiceRegionsAsync (options);
        }

        public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetVoiceRegionAsync (id, options);
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null) {
            return ((IDiscordClient)DiscordClient).GetWebhookAsync (id, options);
        }

        public void Dispose() {
            ((IDiscordClient)DiscordClient).Dispose ();
        }
        // Wrappers' done ya'll.
    }
}
