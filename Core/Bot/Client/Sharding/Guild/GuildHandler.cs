using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class GuildHandler
    {
        private Shard _shard;
        public ulong GuildId { get; private set; }

        public PluginManager Plugins { get; private set; } = new PluginManager();
        public ConfigurationManager Config { get; private set; } = new ConfigurationManager();
        public DataManager Data { get; private set; }

        public GuildHandler (Shard shard, ulong guildId)
        {
            _shard = shard;
            GuildId = guildId;
        }

        public void Initialize ()
        {
            Moduthulhu.Core.Log.Write(Moduthulhu.Core.Log.Type.BOT, $"Initializing GuildHandler for Guild {GetGuild().Name}.");

            Data = new DataManager(GuildId);

            Data.Set("TestValue", GetGuild ().Name);
            Moduthulhu.Core.Log.Write(Moduthulhu.Core.Log.Type.CRITICAL, Data.Get<string>("TestValue"));
        }

        public void Kill ()
        {
            Moduthulhu.Core.Log.Write(Moduthulhu.Core.Log.Type.BOT, $"Killing GuildHandler for Guild {GetGuild().Name}.");

        }

        // ROUTED DISCORD EVENTS //
        #region
        internal async Task OnChannelCreated(SocketChannel x) => await ChannelCreated?.Invoke(x);
        public event Func<SocketChannel, Task> ChannelCreated;

        internal async Task OnChannelDestroyed(SocketChannel x) => await ChannelDestroyed?.Invoke(x);
        public event Func<SocketChannel, Task> ChannelDestroyed;

        internal async Task OnChannelUpdated(SocketChannel x, SocketChannel y) => await ChannelUpdated?.Invoke(x, y);
        public event Func<SocketChannel, SocketChannel, Task> ChannelUpdated;

        internal async Task OnConnected() => await Connected?.Invoke();
        public event Func<Task> Connected; // Done

        internal async Task OnCurrentUserUpdated(SocketSelfUser before, SocketSelfUser after) => await CurrentUserUpdated?.Invoke(before, after);
        public event Func<SocketSelfUser, SocketSelfUser, Task> CurrentUserUpdated; // Done

        internal async Task OnDisconnected(Exception exc) => await Disconnected?.Invoke(exc);
        public event Func<Exception, Task> Disconnected; // Done

        internal async Task OnGuildAvailable() => await GuildAvailable?.Invoke(GetGuild());
        public event Func<SocketGuild, Task> GuildAvailable;

        internal async Task OnGuildUnavailable() => await GuildUnavailable?.Invoke(GetGuild());
        public event Func<SocketGuild, Task> GuildUnavailable;

        internal async Task OnJoinedGuild() => await JoinedGuild?.Invoke(GetGuild());
        public event Func<SocketGuild, Task> JoinedGuild;

        internal async Task OnGuildMembersDownloaded() => await GuildMembersDownloaded?.Invoke(GetGuild());
        public event Func<SocketGuild, Task> GuildMembersDownloaded;

        internal async Task OnGuildMemberUpdated(SocketGuildUser before, SocketGuildUser after) => await GuildMemberUpdated?.Invoke(before, after);
        public event Func<SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated;

        internal async Task OnGuildUpdated(SocketGuild before, SocketGuild after) => await GuildUpdated?.Invoke(before, after);
        public event Func<SocketGuild, SocketGuild, Task> GuildUpdated;

        internal async Task OnLatencyUpdated(int before, int after) => await LatencyUpdated?.Invoke(before, after);
        public event Func<int, int, Task> LatencyUpdated; // Done

        internal async Task OnLog(LogMessage message) => await Log?.Invoke(message);
        public event Func<LogMessage, Task> Log; // Done

        internal async Task OnLoggedIn() => await LoggedIn?.Invoke();
        public event Func<Task> LoggedIn; // Done

        internal async Task OnLoggedOut() => await LoggedOut?.Invoke();
        public event Func<Task> LoggedOut; // Done

        internal async Task OnReady() => await Ready?.Invoke();
        public event Func<Task> Ready; // New

        internal async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) => await MessageDeleted?.Invoke(message, channel);
        public event Func<Cacheable<IMessage, ulong>, ISocketMessageChannel, Task> MessageDeleted;

        internal async Task OnMessageRecieved(SocketMessage message) => await MessageReceived?.Invoke(message);
        public event Func<SocketMessage, Task> MessageReceived;

        internal async Task OnMessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) => await MessageUpdated?.Invoke(before, after, channel);
        public event Func<Cacheable<IMessage, ulong>, SocketMessage, ISocketMessageChannel, Task> MessageUpdated;

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) => await ReactionAdded?.Invoke(message, channel, reaction);
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionAdded;

        internal async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) => await ReactionRemoved?.Invoke(message, channel, reaction);
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> ReactionRemoved;
        
        internal async Task OnReactionsCleared(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel) => await ReactionsCleared?.Invoke(message, channel);
        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, Task> ReactionsCleared;

        internal async Task OnRoleCreated(SocketRole role) => await RoleCreated?.Invoke(role);
        public event Func<SocketRole, Task> RoleCreated;

        internal async Task OnRoleDeleted(SocketRole role) => await RoleDeleted?.Invoke(role);
        public event Func<SocketRole, Task> RoleDeleted;

        internal async Task OnRoleUpdated(SocketRole before, SocketRole after) => await RoleUpdated?.Invoke(before, after);
        public event Func<SocketRole, SocketRole, Task> RoleUpdated;

        internal async Task OnUserBanned(SocketUser user) => await UserBanned?.Invoke(user, GetGuild());
        public event Func<SocketUser, SocketGuild, Task> UserBanned;

        internal async Task OnUserIsTyping(SocketUser user, ISocketMessageChannel channel) => await UserIsTyping?.Invoke(user, channel);
        public event Func<SocketUser, ISocketMessageChannel, Task> UserIsTyping;

        internal async Task OnUserJoined(SocketGuildUser user) => await UserJoined?.Invoke(user);
        public event Func<SocketGuildUser, Task> UserJoined;

        internal async Task OnUserLeft(SocketGuildUser user) => await UserLeft?.Invoke(user);
        public event Func<SocketGuildUser, Task> UserLeft;

        internal async Task OnUserUnbanned(SocketUser user) => await UserUnbanned?.Invoke(user, GetGuild());
        public event Func<SocketUser, SocketGuild, Task> UserUnbanned;

        internal async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after) => await UserVoiceStateUpdated?.Invoke(user, before, after);
        public event Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated;
        // ROUTED DISCORD EVENTS //
        #endregion

        public SocketGuild GetGuild() => _shard.GetGuild(GuildId);
        public SocketGuildUser GetUser(ulong userId) => GetGuild().GetUser(userId);
        public SocketGuildChannel GetChannel(ulong channelId) => GetGuild().GetChannel(channelId);
        public SocketTextChannel GetTextChannel(ulong channelId) => GetChannel(channelId) as SocketTextChannel;
        public SocketVoiceChannel GetVoiceChannel(ulong channelId) => GetChannel(channelId) as SocketVoiceChannel;
        public SocketCategoryChannel GetCategoryChannel(ulong channelId) => GetChannel(channelId) as SocketCategoryChannel;
        public SocketRole GetRole(ulong roleId) => GetGuild().GetRole(roleId);

    }
}
