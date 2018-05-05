using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Modules.Misc.Logging.Extensions;
using System.Linq;

namespace Lomztein.Moduthulhu.Modules.Misc.Logging
{
    public class LoggingModule : ModuleBase {

        public override string Name => "Activity Logging";
        public override string Description => "Logs just about every event the bot sees to console. Few things excluded.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override void Initialize() {
            ParentBotClient.discordClient.ChannelCreated += OnChannelCreated;
            ParentBotClient.discordClient.ChannelDestroyed += OnChannelDestroyed;
            ParentBotClient.discordClient.ChannelUpdated += OnChannelUpdated;
            ParentBotClient.discordClient.GuildAvailable += OnGuildAvailable;
            ParentBotClient.discordClient.GuildMembersDownloaded += OnGuildMembersDownloaded;
            ParentBotClient.discordClient.GuildMemberUpdated += OnGuildMemberUpdated;
            ParentBotClient.discordClient.GuildUnavailable += OnGuildUnavailable;
            ParentBotClient.discordClient.GuildUpdated += OnGuildUpdated;
            ParentBotClient.discordClient.JoinedGuild += OnJoinedGuild;
            ParentBotClient.discordClient.LeftGuild += OnLeftGuild;
            ParentBotClient.discordClient.MessageDeleted += OnMessageDeleted;
            ParentBotClient.discordClient.MessageReceived += OnMessageRecieved;
            ParentBotClient.discordClient.MessageUpdated += OnMessageUpdated;
            ParentBotClient.discordClient.ReactionAdded += OnReactionAdded;
            ParentBotClient.discordClient.ReactionRemoved += OnReactionRemoved;
            ParentBotClient.discordClient.ReactionsCleared += OnReactionsCleared;
            ParentBotClient.discordClient.RoleCreated += OnRoleCreated;
            ParentBotClient.discordClient.RoleDeleted += OnRoleDeleted;
            ParentBotClient.discordClient.RoleUpdated += OnRoleUpdated;
            ParentBotClient.discordClient.UserBanned += OnUserBanned;
            ParentBotClient.discordClient.UserJoined += OnUserJoined;
            ParentBotClient.discordClient.UserLeft += OnUserLeft;
            ParentBotClient.discordClient.UserUnbanned += OnUserUnbanned;
            ParentBotClient.discordClient.UserUpdated += OnUserUpdated;
            ParentBotClient.discordClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.ChannelCreated -= OnChannelCreated;
            ParentBotClient.discordClient.ChannelDestroyed -= OnChannelDestroyed;
            ParentBotClient.discordClient.ChannelUpdated -= OnChannelUpdated;
            ParentBotClient.discordClient.GuildAvailable -= OnGuildAvailable;
            ParentBotClient.discordClient.GuildMembersDownloaded -= OnGuildMembersDownloaded;
            ParentBotClient.discordClient.GuildMemberUpdated -= OnGuildMemberUpdated;
            ParentBotClient.discordClient.GuildUnavailable -= OnGuildUnavailable;
            ParentBotClient.discordClient.GuildUpdated -= OnGuildUpdated;
            ParentBotClient.discordClient.JoinedGuild -= OnJoinedGuild;
            ParentBotClient.discordClient.LeftGuild -= OnLeftGuild;
            ParentBotClient.discordClient.MessageDeleted -= OnMessageDeleted;
            ParentBotClient.discordClient.MessageReceived -= OnMessageRecieved;
            ParentBotClient.discordClient.MessageUpdated -= OnMessageUpdated;
            ParentBotClient.discordClient.ReactionAdded -= OnReactionAdded;
            ParentBotClient.discordClient.ReactionRemoved -= OnReactionRemoved;
            ParentBotClient.discordClient.ReactionsCleared -= OnReactionsCleared;
            ParentBotClient.discordClient.RoleCreated -= OnRoleCreated;
            ParentBotClient.discordClient.RoleDeleted -= OnRoleDeleted;
            ParentBotClient.discordClient.RoleUpdated -= OnRoleUpdated;
            ParentBotClient.discordClient.UserBanned -= OnUserBanned;
            ParentBotClient.discordClient.UserJoined -= OnUserJoined;
            ParentBotClient.discordClient.UserLeft -= OnUserLeft;
            ParentBotClient.discordClient.UserUnbanned -= OnUserUnbanned;
            ParentBotClient.discordClient.UserUpdated -= OnUserUpdated;
            ParentBotClient.discordClient.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after) {
            if (before.VoiceChannel != after.VoiceChannel) {
                Log.Write (Log.Type.USER, $"{user.GetShownName ()} moved from voice channel {before.VoiceChannel.GetPath ()} to {after.VoiceChannel.GetPath ()}");
            } else {
                if (before.IsDeafened != after.IsDeafened)
                    Log.Write (Log.Type.USER, $"{user.GetShownName ()} had deafened toggled: {after.IsDeafened}.");
                if (before.IsMuted != after.IsMuted)
                    Log.Write (Log.Type.USER, $"{user.GetShownName ()} had muted toggled: {after.IsMuted}.");
                if (before.IsSelfDeafened != after.IsSelfDeafened)
                    Log.Write (Log.Type.USER, $"{user.GetShownName ()} had self-deafened toggled: {after.IsSelfDeafened}.");
                if (before.IsSelfMuted != after.IsSelfMuted)
                    Log.Write (Log.Type.USER, $"{user.GetShownName ()} had self-muted toggled: {after.IsSelfMuted}.");
                if (before.IsSuppressed != after.IsSuppressed)
                    Log.Write (Log.Type.USER, $"{user.GetShownName ()} had supressed toggled: {after.IsSuppressed}.");

            }
            return Task.CompletedTask;
        }

        private Task OnUserUpdated(SocketUser arg1, SocketUser arg2) {
            if (arg1.Username != arg2.Username)
                Log.Write (Log.Type.USER, $"{arg1.Username} changed username to {arg2.Username}");
            if (arg1.AvatarId != arg2.AvatarId)
                Log.Write (Log.Type.USER, $"{arg1.Username} changed avatar.");
            return Task.CompletedTask;
        }

        private Task OnUserUnbanned(SocketUser arg1, SocketGuild arg2) {
            Log.Write (Log.Type.USER, arg1.GetShownName () + " has been unbanned from " + arg2.Name);
            return Task.CompletedTask;
        }

        private Task OnUserLeft(SocketGuildUser arg) {
            Log.Write (Log.Type.USER, arg.GetShownName () + " has left " + arg.Guild.Name);
            return Task.CompletedTask;
        }

        private Task OnUserJoined(SocketGuildUser arg) {
            Log.Write (Log.Type.USER, arg.GetShownName () + " has joined " + arg.Guild.Name);
            return Task.CompletedTask;
        }

        private Task OnUserBanned(SocketUser arg1, SocketGuild arg2) {
            Log.Write (Log.Type.USER, arg1.GetShownName () + " has been banned from " + arg2.Name);
            return Task.CompletedTask;
        }

        private Task OnRoleUpdated(SocketRole arg1, SocketRole arg2) {
            if (arg1.Color.RawValue != arg2.Color.RawValue)
                Log.Write (Log.Type.SERVER, "Role " + arg1.GetPath () + " changed colour.");
            if (arg1.Name != arg2.Name)
                Log.Write (Log.Type.SERVER, $"Role {arg1.GetPath ()} changed name to {arg2.Name}");
            if (arg1.Permissions.RawValue != arg2.Permissions.RawValue)
                Log.Write (Log.Type.SERVER, "Role " + arg1.GetPath () + " had permissions changed.");
            if (arg1.IsMentionable != arg2.IsMentionable)
                Log.Write (Log.Type.SERVER, "Role " + arg1.GetPath () + " toggled mentionable: " + arg2.IsMentionable);
            return Task.CompletedTask;
        }

        private Task OnRoleDeleted(SocketRole arg) {
            Log.Write (Log.Type.SERVER, "Role " + arg.GetPath () + " has been deleted.");
            return Task.CompletedTask;
        }

        private Task OnRoleCreated(SocketRole arg) {
            Log.Write (Log.Type.SERVER, "Role " + arg.GetPath () + " has been created.");
            return Task.CompletedTask;
        }

        private Task OnReactionsCleared(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2) {
            Log.Write (Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.GetPath () + " had reactions cleared.");
            return Task.CompletedTask;
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            Log.Write (Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.GetPath () + " had a reaction " + arg3.Emote.Name + " removed by " + arg3.User.ToStringOrNull () + ".");
            return Task.CompletedTask;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            Log.Write (Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.GetPath () + " had a reaction " + arg3.Emote.Name + " added by " + arg3.User.ToStringOrNull () + ".");
            return Task.CompletedTask;
        }

        private Task OnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3) {
            Log.Write (Log.Type.CHAT, "Message " + arg1.Id + " in " + arg3.GetPath () + " has been updated to " + arg2?.Content.ToStringOrNull () + ".");
            return Task.CompletedTask;
        }

        private Task OnMessageRecieved(SocketMessage arg) {
            Log.Write (Log.Type.CHAT, arg.GetPath () + " - " + arg.Content);
            return Task.CompletedTask;
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2) {
            Log.Write (Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.GetPath () + " has been deleted.");
            return Task.CompletedTask;
        }

        private Task OnLeftGuild(SocketGuild arg) {
            Log.Write (Log.Type.BOT, "Bot left guild " + arg.Name);
            return Task.CompletedTask;
        }

        private Task OnJoinedGuild(SocketGuild arg) {
            Log.Write (Log.Type.BOT, "Bot joined guild " + arg.Name);
            return Task.CompletedTask;
        }

        private Task OnGuildUpdated(SocketGuild arg1, SocketGuild arg2) {
            if (arg1.Name != arg2.Name)
                Log.Write (Log.Type.SERVER, $"Guild {arg1.Name} has changed name to {arg2.Name}.");
            if (arg1.OwnerId != arg2.OwnerId)
                Log.Write (Log.Type.SERVER, $"Guild {arg1.Name} has changed owner from {arg1.Owner.GetShownName ()} to {arg2.Owner.GetShownName ()}.");
            if (arg1.IconUrl != arg2.IconUrl)
                Log.Write (Log.Type.SERVER, $"Guild {arg1.Name} has changed icon.");
            return Task.CompletedTask;
        }

        private Task OnGuildUnavailable(SocketGuild arg) {
            Log.Write (Log.Type.SERVER, "Guild " + arg.Name + " became unavailable.");
            return Task.CompletedTask;
        }

        private Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2) {
            if (arg1.Nickname != arg2.Nickname)
                Log.Write (Log.Type.USER, arg1.GetPath () + " changed nickname to " + arg2.GetShownName ());
            if (arg1.GuildPermissions.RawValue != arg2.GuildPermissions.RawValue)
                Log.Write (Log.Type.USER, arg1.GetPath () + " had guild permissions changed.");

            SocketRole added = GetAddedRole (arg1, arg2);

            if (added != null)
                Log.Write (Log.Type.USER, arg1.GetPath () + " had role " + added.GetPath () + " added.");
            else {
                SocketRole removed = GetRemovedRole (arg1, arg2);
                if (removed != null)
                    Log.Write (Log.Type.USER, arg1.GetPath () + " had role " + removed.GetPath () + " removed.");
            }

            return Task.CompletedTask;
        }

        private Task OnGuildMembersDownloaded(SocketGuild arg) {
            Log.Write (Log.Type.SERVER, "Guild " + arg.Name + "'s members downloaded.");
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(SocketGuild arg) {
            Log.Write (Log.Type.SERVER, "Guild " + arg.Name + " became available.");
            return Task.CompletedTask;
        }

        private Task OnChannelUpdated(SocketChannel before, SocketChannel after) {
            if (before is SocketGuildChannel guildBefore && after is SocketGuildChannel guildAfter) {
                if (guildBefore.CategoryId != guildAfter.CategoryId)
                    Log.Write (Log.Type.CHANNEL, $"{guildBefore.GetPath ()} was moved to catagory {guildAfter.Category.GetPath ()}.");
                if (guildBefore.Name != guildAfter.Name)
                    Log.Write (Log.Type.CHANNEL, $"{guildBefore.GetPath ()} changed name to {guildAfter.Name}.");
                if (guildBefore.Position != guildAfter.Position)
                    Log.Write (Log.Type.CHANNEL, $"{guildBefore.GetPath ()} changed position from {guildBefore.Position} to {guildAfter.Position}.");
            }
            
            if (before.Users.Count != after.Users.Count) {
                int difference = before.Users.Count - after.Users.Count;
                if (difference < 0) {
                    Log.Write (Log.Type.CHANNEL, "Channel " + before.GetPath () + " gained " + difference + " new users.");
                } else {
                    Log.Write (Log.Type.CHANNEL, "Channel " + before.GetPath () + " lost " + -difference + " users.");
                }

            } else
                Log.Write (Log.Type.CHANNEL, "Channel " + before.GetPath () + " has changed.");
            return Task.CompletedTask;
        }

        private Task OnChannelDestroyed(SocketChannel arg) {
            Log.Write (Log.Type.CHANNEL, $"{arg.GetPath ()} has been deleted.");
            return Task.CompletedTask;
        }

        private Task OnChannelCreated(SocketChannel arg) {
            Log.Write (Log.Type.CHANNEL, $"{arg.GetPath ()} has been created.");
            return Task.CompletedTask;
        }

        private SocketRole GetAddedRole (SocketGuildUser before, SocketGuildUser after) {
            foreach (SocketRole role in after.Roles)
                if (!before.Roles.Contains (role))
                    return role;
            return null;
        }

        private SocketRole GetRemovedRole (SocketGuildUser before, SocketGuildUser after) {
            foreach (SocketRole role in before.Roles)
                if (!after.Roles.Contains (role))
                    return role;
            return null;
        }
    }
}
