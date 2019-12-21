using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.Config;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class GuildHandler
    {
        public string Name { get; private set; }
        public BotShard Shard { get; private set; }
        public BotClient Client => Shard.BotClient;
        public BotCore Core => Client.Core;
        public ISelfUser BotUser => Shard.Client.CurrentUser;
        public readonly DateTime BootDate;
        public TimeSpan Uptime => DateTime.Now - BootDate;

        public ulong GuildId { get; private set; }

        public PluginManager Plugins { get; private set; }
        public PluginMessenger Messenger { get; private set; }
        public StateManager State { get; private set; }
        public PluginConfig Config { get; private set; }
        public CachedValue<CultureInfo> Culture { get; private set; }
        public Clock Clock { get; private set; }

        public GuildHandler (BotShard shard, ulong guildId)
        {
            Shard = shard;
            GuildId = guildId;
            Plugins = new PluginManager(this);
            Messenger = new PluginMessenger();
            Config = new PluginConfig();
            Clock = new Clock(1, Name);
            Clock.Start();

            Name = GetGuild().Name;
            BootDate = DateTime.Now;
            Plugins.OnPluginUnloaded += Plugins_OnPluginUnloaded;

            Culture = new CachedValue<CultureInfo>(new IdentityKeyJsonRepository("pluginconfig"), GuildId, "Culture", () => new CultureInfo("en-US"));
        }

        private void Plugins_OnPluginUnloaded(IPlugin plugin)
        {
            Messenger.Clear(Plugin.GetFullName(plugin.GetType ()));
            Config.Clear(Plugin.GetFullName(plugin.GetType ()));
        }

        public void AddPluginStateAttribute(string target, string addedHeader, string removedHeader, string mutatedHeader, string name, string desc) => Plugins.State.AddAttribute(target, addedHeader, removedHeader, mutatedHeader, name, desc);
        public void AddPluginStateAttribute(string target, string addedHeader, string removedHeader, string name, string desc) => Plugins.State.AddAttribute(target, addedHeader, removedHeader, string.Empty, name, desc);

        public bool IsBotAdministrator(ulong userId) => Shard.BotClient.IsBotAdministrator(userId);

        public void Initialize ()
        {
            Moduthulhu.Core.Log.Write(Moduthulhu.Core.Log.Type.BOT, $"Initializing GuildHandler for Guild {GetGuild().Name}.");
            Plugins.LoadPlugins();
        }

        public void Kill ()
        {
            Moduthulhu.Core.Log.Write(Moduthulhu.Core.Log.Type.BOT, $"Killing GuildHandler for guild {Name}.");
            Plugins.ShutdownPlugins();
        }

        public override string ToString ()
        {
            return $"{GetGuild().Name} (Users: {GetGuild().MemberCount}, Plugins: {Plugins.GetActivePlugins().Length}, Uptime {Uptime})";
        }

        // ROUTED DISCORD EVENTS //
        #region
        internal async Task OnChannelCreated(SocketChannel x) => await (ChannelCreated?.Invoke(x) ?? Task.CompletedTask);
        public event Func<SocketChannel, Task> ChannelCreated;

        internal async Task OnChannelDestroyed(SocketChannel x) => await (ChannelDestroyed?.Invoke(x) ?? Task.CompletedTask);
        public event Func<SocketChannel, Task> ChannelDestroyed;

        internal async Task OnChannelUpdated(SocketChannel x, SocketChannel y) => await (ChannelUpdated?.Invoke(x, y) ?? Task.CompletedTask);
        public event Func<SocketChannel, SocketChannel, Task> ChannelUpdated;

        internal async Task OnConnected() => await (Connected?.Invoke() ?? Task.CompletedTask);
        public event Func<Task> Connected; // Done

        internal async Task OnCurrentUserUpdated(SocketSelfUser before, SocketSelfUser after) => await (CurrentUserUpdated?.Invoke(before, after) ?? Task.CompletedTask);
        public event Func<SocketSelfUser, SocketSelfUser, Task> CurrentUserUpdated; // Done

        internal async Task OnDisconnected(Exception exc) => await (Disconnected?.Invoke(exc) ?? Task.CompletedTask);
        public event Func<Exception, Task> Disconnected; // Done

        internal async Task OnGuildAvailable() => await (GuildAvailable?.Invoke(GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketGuild, Task> GuildAvailable;

        internal async Task OnGuildUnavailable() => await (GuildUnavailable?.Invoke(GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketGuild, Task> GuildUnavailable;

        internal async Task OnJoinedGuild() => await (JoinedGuild?.Invoke(GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketGuild, Task> JoinedGuild;
        internal async Task OnLeftGuild() => await (LeftGuild?.Invoke(GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketGuild, Task> LeftGuild;

        internal async Task OnGuildMembersDownloaded() => await (GuildMembersDownloaded?.Invoke(GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketGuild, Task> GuildMembersDownloaded;

        internal async Task OnGuildMemberUpdated(SocketGuildUser before, SocketGuildUser after) => await (GuildMemberUpdated?.Invoke(before, after) ?? Task.CompletedTask);
        public event Func<SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated;

        internal async Task OnGuildUpdated(SocketGuild before, SocketGuild after) => await (GuildUpdated?.Invoke(before, after) ?? Task.CompletedTask);
        public event Func<SocketGuild, SocketGuild, Task> GuildUpdated;

        internal async Task OnLatencyUpdated(int before, int after) => await (LatencyUpdated?.Invoke(before, after) ?? Task.CompletedTask);
        public event Func<int, int, Task> LatencyUpdated; // Done

        internal async Task OnLog(LogMessage message) => await (Log?.Invoke(message) ?? Task.CompletedTask);
        public event Func<LogMessage, Task> Log; // Done

        internal async Task OnLoggedIn() => await (LoggedIn?.Invoke() ?? Task.CompletedTask);
        public event Func<Task> LoggedIn; // Done

        internal async Task OnLoggedOut() => await (LoggedOut?.Invoke() ?? Task.CompletedTask);
        public event Func<Task> LoggedOut; // Done

        internal async Task OnReady() => await (Ready?.Invoke() ?? Task.CompletedTask);
        public event Func<Task> Ready; // New

        internal async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) => await (MessageDeleted?.Invoke(message, channel) ?? Task.CompletedTask);
        public event Func<Cacheable<IMessage, ulong>, ISocketMessageChannel, Task> MessageDeleted;

        internal async Task OnMessageRecieved(SocketMessage message) => await (MessageReceived?.Invoke(message) ?? Task.CompletedTask);
        public event Func<SocketMessage, Task> MessageReceived;

        internal async Task OnMessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) => await (MessageUpdated?.Invoke(before, after, channel) ?? Task.CompletedTask);
        public event Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task> MessageUpdated;

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) => await (ReactionAdded?.Invoke(message, channel, reaction) ?? Task.CompletedTask);
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;

        internal async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) => await (ReactionRemoved?.Invoke(message, channel, reaction) ?? Task.CompletedTask);
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;
        
        internal async Task OnReactionsCleared(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel) => await (ReactionsCleared?.Invoke(message, channel) ?? Task.CompletedTask);
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, Task> ReactionsCleared;

        internal async Task OnRoleCreated(SocketRole role) => await (RoleCreated?.Invoke(role) ?? Task.CompletedTask);
        public event Func<SocketRole, Task> RoleCreated;

        internal async Task OnRoleDeleted(SocketRole role) => await (RoleDeleted?.Invoke(role) ?? Task.CompletedTask);
        public event Func<SocketRole, Task> RoleDeleted;

        internal async Task OnRoleUpdated(SocketRole before, SocketRole after) => await (RoleUpdated?.Invoke(before, after) ?? Task.CompletedTask);
        public event Func<SocketRole, SocketRole, Task> RoleUpdated;

        internal async Task OnUserBanned(SocketUser user) => await (UserBanned?.Invoke(user, GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketUser, SocketGuild, Task> UserBanned;

        internal async Task OnUserIsTyping(SocketUser user, ISocketMessageChannel channel) => await (UserIsTyping?.Invoke(user, channel) ?? Task.CompletedTask);
        public event Func<SocketUser, ISocketMessageChannel, Task> UserIsTyping;

        internal async Task OnUserJoined(SocketGuildUser user) => await (UserJoined?.Invoke(user) ?? Task.CompletedTask);
        public event Func<SocketGuildUser, Task> UserJoined;

        internal async Task OnUserLeft(SocketGuildUser user) => await (UserLeft?.Invoke(user) ?? Task.CompletedTask);
        public event Func<SocketGuildUser, Task> UserLeft;

        internal async Task OnUserUnbanned(SocketUser user) => await (UserUnbanned?.Invoke(user, GetGuild()) ?? Task.CompletedTask);
        public event Func<SocketUser, SocketGuild, Task> UserUnbanned;

        internal async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after) => await (UserVoiceStateUpdated?.Invoke(user, before, after) ?? Task.CompletedTask);
        public event Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated;
        // ROUTED DISCORD EVENTS //
        #endregion

        // DISCORD GETTERS / FINDERS
        #region
        public SocketGuild GetGuild() => Shard.GetGuild(GuildId);

        public SocketGuildUser FindUser(ulong userId) => GetGuild().GetUser(userId);
        public SocketGuildUser FindUser(Predicate<SocketGuildUser> predicate) => GetGuild().Users.FirstOrDefault(x => predicate(x));
        public SocketGuildUser FindUser(string name) => FindUser (x => ObjectNameMatches(name, string.IsNullOrWhiteSpace (x.Nickname) ? x.Username : x.Nickname));

        public SocketGuildChannel FindChannel(ulong channelId) => GetGuild().GetChannel(channelId);
        public SocketGuildChannel FindChannel(Predicate<SocketGuildChannel> predicate) => GetGuild().Channels.FirstOrDefault(x => predicate(x));
        public SocketGuildChannel FindChannel(string name) => FindChannel(x => ObjectNameMatches(name, x.Name));

        public SocketTextChannel FindTextChannel(ulong channelId) => FindChannel(channelId) as SocketTextChannel;
        public SocketTextChannel FindTextChannel(Predicate<SocketTextChannel> predicate) => GetGuild().TextChannels.FirstOrDefault(x => predicate(x));
        public SocketTextChannel FindTextChannel(string name) => FindTextChannel(x => ObjectNameMatches(name, x.Name));

        public SocketVoiceChannel FindVoiceChannel(ulong channelId) => FindChannel(channelId) as SocketVoiceChannel;
        public SocketVoiceChannel FindVoiceChannel(Predicate<SocketVoiceChannel> predicate) => GetGuild().VoiceChannels.FirstOrDefault(x => predicate(x));
        public SocketVoiceChannel FindVoiceChannel(string name) => GetGuild().VoiceChannels.FirstOrDefault(x => ObjectNameMatches(name, x.Name));

        public SocketCategoryChannel FindCategoryChannel(ulong channelId) => FindChannel(channelId) as SocketCategoryChannel;
        public SocketCategoryChannel FindCategoryChannel(Predicate<SocketCategoryChannel> predicate) => GetGuild().CategoryChannels.FirstOrDefault(x => predicate(x));
        public SocketCategoryChannel FindCategoryChannel(string name) => FindCategoryChannel(x => ObjectNameMatches(name, x.Name));

        public SocketRole FindRole(ulong roleId) => GetGuild().GetRole(roleId);
        public SocketRole FindRole(Predicate<SocketRole> predicate) => GetGuild().Roles.FirstOrDefault(x => predicate(x));
        public SocketRole FindRole(string name) => FindRole(x => ObjectNameMatches(name, x.Name));

        public SocketGuildUser GetUser(ulong userId) => ThrowIfNull(FindUser(userId), "That user does not exist on this server.");
        public SocketGuildUser GetUser(Predicate<SocketGuildUser> predicate) => ThrowIfNull(FindUser (predicate), "No matching user could be found on this server.");
        public SocketGuildUser GetUser(string name) => ThrowIfNull(FindUser(name), $"User '{name}' could not be found on this server.");

        public SocketGuildChannel GetChannel(ulong channelId) => ThrowIfNull(FindChannel(channelId), "That channel does not exist on this server.");
        public SocketGuildChannel GetChannel(Predicate<SocketGuildChannel> predicate) => ThrowIfNull(FindChannel(predicate), "No matching channel could be found on this server.");
        public SocketGuildChannel GetChannel(string name) => ThrowIfNull(FindChannel(name), $"Channel '{name}' could not be found on this server.");

        public SocketTextChannel GetTextChannel(ulong channelId) => ThrowIfNull(FindTextChannel(channelId), "That text channel does not exist on this server.");
        public SocketTextChannel GetTextChannel(Predicate<SocketTextChannel> predicate) => ThrowIfNull(FindTextChannel(predicate), "No matching text channel could be found on this server.");
        public SocketTextChannel GetTextChannel(string name) => ThrowIfNull(FindTextChannel(name), $"Text channel '{name}' could not be found on this server.");

        public SocketVoiceChannel GetVoiceChannel(ulong channelId) => ThrowIfNull(FindVoiceChannel(channelId), "That voice channel does not exist on this server.");
        public SocketVoiceChannel GetVoiceChannel(Predicate<SocketVoiceChannel> predicate) => ThrowIfNull(FindVoiceChannel(predicate), "No matching voice channel could be found on this server.");
        public SocketVoiceChannel GetVoiceChannel(string name) => ThrowIfNull(FindVoiceChannel(name), $"Voice channel '{name}' could not be found on this server.");

        public SocketCategoryChannel GetCategoryChannel(ulong channelId) => ThrowIfNull(FindCategoryChannel(channelId), "That category does not exist on this server.");
        public SocketCategoryChannel GetCategoryChannel(Predicate<SocketCategoryChannel> predicate) => ThrowIfNull(FindCategoryChannel(predicate), "No matching categories could be found on this server.");
        public SocketCategoryChannel GetCategoryChannel(string name) => ThrowIfNull(FindCategoryChannel(name), $"Category '{name}' could not be found on this server.");

        public SocketRole GetRole(ulong roleId) => ThrowIfNull(FindRole(roleId), "That role does not exist on this server.");
        public SocketRole GetRole(Predicate<SocketRole> predicate) => ThrowIfNull(FindRole(predicate), $"No matching role could be found on this server.");
        public SocketRole GetRole(string name) => ThrowIfNull(FindRole(name), $"Role '{name}' could not be found on this server.");
        #endregion

        public IEnumerable<ulong> FilterMissingUsers(IEnumerable<ulong> enumerable) => enumerable.Where(x => FindUser(x) != null);
        public IEnumerable<string> FilterMissingUsers(IEnumerable<string> enumerable) => enumerable.Where(x => FindUser(x) != null);
        public IEnumerable<ulong> FilterMissingChannels(IEnumerable<ulong> enumerable) => enumerable.Where(x => FindChannel(x) != null);
        public IEnumerable<string> FilterMissingChannels(IEnumerable<string> enumerable) => enumerable.Where(x => FindChannel(x) != null);
        public IEnumerable<ulong> FilterMissingRoles(IEnumerable<ulong> enumerable) => enumerable.Where(x => FindRole(x) != null);
        public IEnumerable<string> FilterMissingRoles(IEnumerable<string> enumerable) => enumerable.Where(x => FindRole(x) != null);

        private static bool ObjectNameMatches (string search, string name)
        {
            return name.ToUpperInvariant ().Contains(search.ToUpperInvariant (), StringComparison.Ordinal);
        }

        private T ThrowIfNull<T> (T value, string message)
        {
            if (value == null)
            {
                throw new ArgumentException(message);
            }
            return value;
        }

        public bool HasPermission(GuildPermission permission) => GetUser(BotUser.Id).GuildPermissions.Has(permission);
        public void AssertPermission (GuildPermission permission)
        {
            if (!HasPermission (permission))
            {
                throw new MissingPermissionException($"Bot does not have {nameof (permission)} '{permission}'.");
            }
        }


        public bool HasChannelPermission(ChannelPermission permission, ulong channelId) => GetUser(BotUser.Id).GetPermissions(GetChannel(channelId)).Has(permission);
        public void AssertChannelPermission (ChannelPermission permission, ulong channelId)
        {
            IGuildChannel channel = GetChannel(channelId);
            if (!HasChannelPermission (permission, channelId))
            {
                throw new MissingPermissionException($"Bot does not have channel {nameof (permission)} '{permission} in channel '{channel.Name}'");
            }
        }
    }
}
