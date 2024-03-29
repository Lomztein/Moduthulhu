﻿using Discord;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Extensions;
using System.Linq;
using Lomztein.AdvDiscordCommands.Extensions;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    [Descriptor ("Lomztein", "Logger", "Simple plugin that logs whatever is going on for debugging purposes. Planned to be queryable to get all logs relating to an individual user or server.", "1.0.0")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Logger/LoggerPlugin.cs")]
    [GDPR(GDPRCompliance.None, "Logs may be stored in temporary files for inspection, but are not stored in a permanent database.", "Logs include user names, user messages, server names, channel names, role names, and more.")]
    public class LoggerPlugin : PluginBase {

        public override void Initialize() {
            GuildHandler.ChannelCreated += OnChannelCreated;
            GuildHandler.ChannelDestroyed += OnChannelDestroyed;
            GuildHandler.ChannelUpdated += OnChannelUpdated;
            GuildHandler.GuildAvailable += OnGuildAvailable;
            GuildHandler.GuildMembersDownloaded += OnGuildMembersDownloaded;
            GuildHandler.GuildMemberUpdated += OnGuildMemberUpdated;
            GuildHandler.GuildUnavailable += OnGuildUnavailable;
            GuildHandler.GuildUpdated += OnGuildUpdated;
            GuildHandler.JoinedGuild += OnJoinedGuild;
            GuildHandler.LeftGuild += OnLeftGuild;
            GuildHandler.MessageDeleted += OnMessageDeleted;
            GuildHandler.MessageReceived += OnMessageRecieved;
            GuildHandler.MessageUpdated += OnMessageUpdated;
            GuildHandler.ReactionAdded += OnReactionAdded;
            GuildHandler.ReactionRemoved += OnReactionRemoved;
            GuildHandler.ReactionsCleared += OnReactionsCleared;
            GuildHandler.RoleCreated += OnRoleCreated;
            GuildHandler.RoleDeleted += OnRoleDeleted;
            GuildHandler.RoleUpdated += OnRoleUpdated;
            GuildHandler.UserBanned += OnUserBanned;
            GuildHandler.UserJoined += OnUserJoined;
            GuildHandler.UserLeft += OnUserLeft;
            GuildHandler.UserUnbanned += OnUserUnbanned;
            GuildHandler.UserVoiceStateUpdated += OnUserVoiceStateUpdated;

            AddGeneralFeaturesStateAttribute("SpookyLogging", "Spooky logging of all ongoings.");
        }

        public override void Shutdown() {
            GuildHandler.ChannelCreated -= OnChannelCreated;
            GuildHandler.ChannelDestroyed -= OnChannelDestroyed;
            GuildHandler.ChannelUpdated -= OnChannelUpdated;
            GuildHandler.GuildAvailable -= OnGuildAvailable;
            GuildHandler.GuildMembersDownloaded -= OnGuildMembersDownloaded;
            GuildHandler.GuildMemberUpdated -= OnGuildMemberUpdated;
            GuildHandler.GuildUnavailable -= OnGuildUnavailable;
            GuildHandler.GuildUpdated -= OnGuildUpdated;
            GuildHandler.JoinedGuild -= OnJoinedGuild;
            GuildHandler.LeftGuild -= OnLeftGuild;
            GuildHandler.MessageDeleted -= OnMessageDeleted;
            GuildHandler.MessageReceived -= OnMessageRecieved;
            GuildHandler.MessageUpdated -= OnMessageUpdated;
            GuildHandler.ReactionAdded -= OnReactionAdded;
            GuildHandler.ReactionRemoved -= OnReactionRemoved;
            GuildHandler.ReactionsCleared -= OnReactionsCleared;
            GuildHandler.RoleCreated -= OnRoleCreated;
            GuildHandler.RoleDeleted -= OnRoleDeleted;
            GuildHandler.RoleUpdated -= OnRoleUpdated;
            GuildHandler.UserBanned -= OnUserBanned;
            GuildHandler.UserJoined -= OnUserJoined;
            GuildHandler.UserLeft -= OnUserLeft;
            GuildHandler.UserUnbanned -= OnUserUnbanned;
            GuildHandler.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
        }

        private static Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after) {
            if (before.VoiceChannel != after.VoiceChannel) {
                Core.Log.Write (Core.Log.Type.USER, $"{user.GetShownName ()} moved from voice channel {before.VoiceChannel.GetPath ()} to {after.VoiceChannel.GetPath ()}");
            } else {
                if (before.IsDeafened != after.IsDeafened)
                {
                    Core.Log.Write(Core.Log.Type.USER, $"{user.GetShownName()} had deafened toggled: {after.IsDeafened}.");
                }
                if (before.IsMuted != after.IsMuted)
                {
                    Core.Log.Write(Core.Log.Type.USER, $"{user.GetShownName()} had muted toggled: {after.IsMuted}.");
                }
                if (before.IsSelfDeafened != after.IsSelfDeafened)
                {
                    Core.Log.Write(Core.Log.Type.USER, $"{user.GetShownName()} had self-deafened toggled: {after.IsSelfDeafened}.");
                }
                if (before.IsSelfMuted != after.IsSelfMuted)
                {
                    Core.Log.Write(Core.Log.Type.USER, $"{user.GetShownName()} had self-muted toggled: {after.IsSelfMuted}.");
                }
                if (before.IsSuppressed != after.IsSuppressed)
                {
                    Core.Log.Write(Core.Log.Type.USER, $"{user.GetShownName()} had supressed toggled: {after.IsSuppressed}.");
                }
            }

            return Task.CompletedTask;
        }

        private static Task OnUserUnbanned(SocketUser arg1, SocketGuild arg2) {
            Core.Log.Write (Core.Log.Type.USER, arg1.GetShownName () + " has been unbanned from " + arg2.Name);
            return Task.CompletedTask;
        }

        private Task OnUserLeft(SocketUser arg) {
            Core.Log.Write (Core.Log.Type.USER, arg.GetShownName () + " has left " + GuildHandler.GetGuild());
            return Task.CompletedTask;
        }

        private static Task OnUserJoined(SocketGuildUser arg) {
            Core.Log.Write (Core.Log.Type.USER, arg.GetShownName () + " has joined " + arg.Guild.Name);
            return Task.CompletedTask;
        }

        private static Task OnUserBanned(SocketUser arg1, SocketGuild arg2) {
            Core.Log.Write (Core.Log.Type.USER, arg1.GetShownName () + " has been banned from " + arg2.Name);
            return Task.CompletedTask;
        }

        private static Task OnRoleUpdated(SocketRole arg1, SocketRole arg2) {
            if (arg1.Color.RawValue != arg2.Color.RawValue)
            {
                Core.Log.Write(Core.Log.Type.SERVER, "Role " + arg1.GetPath() + " changed colour.");
            }
            if (arg1.Name != arg2.Name)
            {
                Core.Log.Write(Core.Log.Type.SERVER, $"Role {arg1.GetPath()} changed name to {arg2.Name}");
            }
            if (arg1.Permissions.RawValue != arg2.Permissions.RawValue)
            {
                Core.Log.Write(Core.Log.Type.SERVER, "Role " + arg1.GetPath() + " had permissions changed.");
            }
            if (arg1.IsMentionable != arg2.IsMentionable)
            {
                Core.Log.Write(Core.Log.Type.SERVER, "Role " + arg1.GetPath() + " toggled mentionable: " + arg2.IsMentionable);
            }
            return Task.CompletedTask;
        }

        private static Task OnRoleDeleted(SocketRole arg) {
            Core.Log.Write (Core.Log.Type.SERVER, "Role " + arg.GetPath () + " has been deleted.");
            return Task.CompletedTask;
        }

        private static Task OnRoleCreated(SocketRole arg) {
            Core.Log.Write (Core.Log.Type.SERVER, "Role " + arg.GetPath () + " has been created.");
            return Task.CompletedTask;
        }

        private static async Task OnReactionsCleared(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2) {
            await arg2.GetOrDownloadAsync();
            Core.Log.Write (Core.Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.Value.GetPath () + " had reactions cleared.");
        }

        private static async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3) {
            await arg2.GetOrDownloadAsync();
            Core.Log.Write (Core.Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.Value.GetPath () + " had a reaction " + arg3.Emote.Name + " removed by " + arg3.User.Value?.ToStringOrNull () + ".");
        }

        private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3) {
            await arg2.GetOrDownloadAsync();
            Core.Log.Write (Core.Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.Value.GetPath () + " had a reaction " + arg3.Emote.Name + " added by " + arg3.User.Value?.ToStringOrNull () + ".");
        }

        private static Task OnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3) {
            Core.Log.Write (Core.Log.Type.CHAT, "Message " + arg1.Id + " in " + arg3.GetPath () + " has been updated to " + arg2?.Content.ToStringOrNull () + ".");
            return Task.CompletedTask;
        }

        private static Task OnMessageRecieved(SocketMessage arg) {
            Core.Log.Write (Core.Log.Type.CHAT, arg.GetPath () + " - " + arg.Content);
            return Task.CompletedTask;
        }

        private static async Task OnMessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2) {
            await arg2.GetOrDownloadAsync();
            Core.Log.Write (Core.Log.Type.CHAT, "Message " + arg1.Id + " in " + arg2.Value.GetPath () + " has been deleted.");
        }

        private static Task OnLeftGuild(SocketGuild arg) {
            Core.Log.Write (Core.Log.Type.BOT, "Bot left guild " + arg.Name);
            return Task.CompletedTask;
        }

        private static Task OnJoinedGuild(SocketGuild arg) {
            Core.Log.Write (Core.Log.Type.BOT, "Bot joined guild " + arg.Name);
            return Task.CompletedTask;
        }

        private static Task OnGuildUpdated(SocketGuild arg1, SocketGuild arg2) {
            if (arg1.Name != arg2.Name)
            {
                Core.Log.Write(Core.Log.Type.SERVER, $"Guild {arg1.Name} has changed name to {arg2.Name}.");
            }
            if (arg1.OwnerId != arg2.OwnerId)
            {
                Core.Log.Write(Core.Log.Type.SERVER, $"Guild {arg1.Name} has changed owner from {arg1.Owner.GetShownName()} to {arg2.Owner.GetShownName()}.");
            }
            if (arg1.IconUrl != arg2.IconUrl)
            {
                Core.Log.Write(Core.Log.Type.SERVER, $"Guild {arg1.Name} has changed icon.");
            }
            return Task.CompletedTask;
        }

        private static Task OnGuildUnavailable(SocketGuild arg) {
            Core.Log.Write (Core.Log.Type.SERVER, "Guild " + arg.Name + " became unavailable.");
            return Task.CompletedTask;
        }

        private static async Task OnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2) {
            var before = await arg1.GetOrDownloadAsync();
            if (before.Nickname != arg2.Nickname)
            {
                Core.Log.Write(Core.Log.Type.USER, before.GetPath() + " changed nickname to " + arg2.GetShownName());
            }
            if (before.GuildPermissions.RawValue != arg2.GuildPermissions.RawValue)
            {
                Core.Log.Write(Core.Log.Type.USER, before.GetPath() + " had guild permissions changed.");
            }

            SocketRole added = GetAddedRole (before, arg2);

            if (added != null)
            {
                Core.Log.Write(Core.Log.Type.USER, before.GetPath() + " had role " + added.GetPath() + " added.");
            }
            else {
                SocketRole removed = GetRemovedRole (before, arg2);
                if (removed != null)
                {
                    Core.Log.Write(Core.Log.Type.USER, before.GetPath() + " had role " + removed.GetPath() + " removed.");
                }
            }
        }

        private static Task OnGuildMembersDownloaded(SocketGuild arg) {
            Core.Log.Write (Core.Log.Type.SERVER, "Guild " + arg.Name + "'s members downloaded.");
            return Task.CompletedTask;
        }

        private static Task OnGuildAvailable(SocketGuild arg) {
            Core.Log.Write (Core.Log.Type.SERVER, "Guild " + arg.Name + " became available.");
            return Task.CompletedTask;
        }

        private static Task OnChannelUpdated(SocketChannel before, SocketChannel after) {
            if (before is SocketGuildChannel guildBefore && after is SocketGuildChannel guildAfter) {
                if (guildBefore.Name != guildAfter.Name)
                {
                    Core.Log.Write(Core.Log.Type.CHANNEL, $"{guildBefore.GetPath()} changed name to {guildAfter.Name}.");
                }
                if (guildBefore.Position != guildAfter.Position)
                {
                    Core.Log.Write(Core.Log.Type.CHANNEL, $"{guildBefore.GetPath()} changed position from {guildBefore.Position} to {guildAfter.Position}.");
                }
            }
            
            if (before.Users.Count != after.Users.Count) {
                int difference = before.Users.Count - after.Users.Count;
                if (difference < 0) {
                    Core.Log.Write (Core.Log.Type.CHANNEL, "Channel " + before.GetPath () + " gained " + difference + " new users.");
                } else {
                    Core.Log.Write (Core.Log.Type.CHANNEL, "Channel " + before.GetPath () + " lost " + -difference + " users.");
                }
            } else
            {
                Core.Log.Write(Core.Log.Type.CHANNEL, "Channel " + before.GetPath() + " has changed.");
            }
            return Task.CompletedTask;
        }

        private static Task OnChannelDestroyed(SocketChannel arg) {
            Core.Log.Write (Core.Log.Type.CHANNEL, $"{arg.GetPath ()} has been deleted.");
            return Task.CompletedTask;
        }

        private static Task OnChannelCreated(SocketChannel arg) {
            Core.Log.Write (Core.Log.Type.CHANNEL, $"{arg.GetPath ()} has been created.");
            return Task.CompletedTask;
        }

        private static SocketRole GetAddedRole (SocketGuildUser before, SocketGuildUser after) {
            foreach (SocketRole role in after.Roles)
            {
                if (!before.Roles.Contains(role))
                {
                    return role;
                }
            }
            return null;
        }

        private static SocketRole GetRemovedRole (SocketGuildUser before, SocketGuildUser after) {
            foreach (SocketRole role in before.Roles)
            {
                if (!after.Roles.Contains(role))
                {
                    return role;
                }
            }
            return null;
        }
    }
}
