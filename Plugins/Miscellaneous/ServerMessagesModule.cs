using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Discord.Rest;
using System.Linq;
using Discord;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using System.Collections.Generic;

namespace Lomztein.Moduthulhu.Modules.ServerMessages {

    [Descriptor ("Lomztein", "Server Messages", "Sends out a variety of messages in response to server events, such as new member joins.")]
    public class ServerMessagesModule : PluginBase {

        private CachedValue<ulong> _channelId;
        private CachedValue<List<string>> _onJoinedNewGuild;
        private CachedValue<List<string>> _onUserJoinedGuild;
        private CachedValue<List<string>> _onUserJoinedGuildByInvite;
        private CachedValue<List<string>> _onUserLeftGuild;
        private CachedValue<List<string>> _onUserBannedFromGuild;
        private CachedValue<List<string>> _onUserUnbannedFromGuild;
        private CachedValue<List<string>> _onUserNameChanged;

        private InviteHandler _inviteHandler;

        public override void Initialize() {
            GuildHandler.JoinedGuild += OnJoinedNewGuild;
            GuildHandler.UserJoined += OnUserJoinedGuild;
            GuildHandler.UserLeft += OnUserLeftGuild;
            GuildHandler.UserBanned += OnUserBannedFromGuild;
            GuildHandler.UserUnbanned += OnUserUnbannedFromGuild;
            GuildHandler.GuildMemberUpdated += OnGuildMemberUpdated;

            _channelId = GetConfigCache("MessageChannelID", x => x.GetGuild().TextChannels.FirstOrDefault().ZeroIfNull());
            _onJoinedNewGuild = GetConfigCache("OnJoinedNewGuild", x => new List<string> { "**[BOTNAME]** here, arrived ready and primed to crash at inoppertune times!" });
            _onUserJoinedGuild = GetConfigCache("OnUserJoinedGuild", x => new List<string> { "**[USERNAME]** has joined this server!" });
            _onUserJoinedGuildByInvite = GetConfigCache("OnUserJoinedNewGuildByInvite", x => new List<string> { "**[USERNAME]** has joined this server through the help of **[INVITERNAME]**!" });
            _onUserLeftGuild = GetConfigCache("OnUserLeftGuild", x => new List<string> { "**[USERNAME]** has left this server. :C" });
            _onUserBannedFromGuild = GetConfigCache("OnUserBannedFromGuild", x => new List<string> { "**[USERNAME]** has been banned from this server." });
            _onUserUnbannedFromGuild = GetConfigCache("OnUserUnbannedFromGuild", x => new List<string> { "**[USERNAME]** has been unbanned from this server!" });
            _onUserNameChanged = GetConfigCache("OnUserNameChanged", x => new List<string> { "**[USERNAME] changed their name to **[NEWNAME]**!" });

            AddConfigInfoForMessage(_onJoinedNewGuild, "On Bot Joined");
            AddConfigInfoForMessage(_onUserJoinedGuild, "On New Member");
            AddConfigInfoForMessage(_onUserLeftGuild, "On Member Left");
            AddConfigInfoForMessage(_onUserBannedFromGuild, "On Member Banned");
            AddConfigInfoForMessage(_onUserUnbannedFromGuild, "On Member Unbanned");
            AddConfigInfoForMessage(_onUserNameChanged, "On Name Changed");
            AddConfigInfoForMessage(_onUserJoinedGuildByInvite, "On New Member Invited");

            AddConfigInfo("Set Message Channel", "Set channel", new Action<int, SocketTextChannel>((x, y) => _channelId.SetValue (y.Id)), () => $"Message channel set to {GuildHandler.GetTextChannel(_channelId.GetValue ()).Name}", "Index", "Channel");
            AddConfigInfo("Set Message Channel", "Set channel", new Action<int, ulong>((x, y) => _channelId.SetValue (y)), () => $"Message channel set to {GuildHandler.GetTextChannel(_channelId.GetValue()).Name}", "Index", "Channel");
            AddConfigInfo("Set Message Channel", "Set channel", new Action<int, string>((x, y) => _channelId.SetValue (GuildHandler.FindTextChannel (y).Id)), () => $"Message channel set to {GuildHandler.GetTextChannel(_channelId.GetValue()).Name}", "Index", "Channel");

            if (HasPermission (GuildPermission.ManageGuild))
            {
                _inviteHandler = new InviteHandler(GuildHandler);
                _ = _inviteHandler.Intialize().ConfigureAwait(false);
            }
        }

        private void AddConfigInfoForMessage (CachedValue<List<string>> message, string name)
        {
            AddConfigInfo(name, "Add a message", new Action<int>(x => { message.GetValue().RemoveAt (x); message.Store(); }), () => $"Removed {name} message.", "Index");
            AddConfigInfo(name, "The above one actually removes lol will fix later", new Action<string>(x => { message.GetValue().Add(x); message.Store(); }), () => $"Added new {name} message.", "Message");
            AddConfigInfo(name, "Display messages", () => $"Current '{name}' messages:\n{string.Join('\n', message.GetValue().ToArray ())}");
        }

        private async Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2) {
            await CheckAndAnnounceNameChange (arg1, arg2);
        }

        private async Task CheckAndAnnounceNameChange(SocketGuildUser before, SocketGuildUser after) {

            if (before == null || after == null)
            {
                return;
            }

            if (before.GetShownName () != after.GetShownName ()) {
                await SendMessage (_onUserNameChanged, "[USERNAME]", before.GetShownName (), "[NEWNAME]", after.GetShownName ());
            }
        }

        private async Task OnUserUnbannedFromGuild(SocketUser user, SocketGuild guild) {
            await SendMessage (_onUserUnbannedFromGuild, "[USERNAME]", user.GetShownName ());
        }

        private async Task OnUserLeftGuild(SocketGuildUser user) {
            await SendMessage (_onUserLeftGuild, "[USERNAME]", user.GetShownName ());
        }

        private async Task OnUserBannedFromGuild(SocketUser user, SocketGuild guild) {
            await SendMessage (_onUserBannedFromGuild, "[USERNAME]", user.GetShownName ());
        }

        private async Task OnUserJoinedGuild(SocketGuildUser user) {
            await OnUserJoinedGuildAsync (user);
        }

        private async Task OnUserJoinedGuildAsync (SocketGuildUser user) {
            RestInviteMetadata invite = await _inviteHandler.FindInviter (user.Guild);
            if (invite == null || !HasPermission (GuildPermission.ManageGuild))
            {
                await SendMessage(_onUserJoinedGuild, "[USERNAME]", user.GetShownName());
            }
            else
            {
                await SendMessage(_onUserJoinedGuildByInvite, "[USERNAME]", user.GetShownName(), "[INVITERNAME]", invite.Inviter.GetShownName());
            }
        }

        private async Task OnJoinedNewGuild(SocketGuild guild) {
            await SendMessage (_onJoinedNewGuild, "[BOTNAME]", GuildHandler.BotUser.GetShownName ());
            if (HasPermission (GuildPermission.ManageGuild))
            {
                await _inviteHandler.UpdateData(guild);
            }
        }

        private async Task SendMessage (CachedValue<List<string>> messages, params string[] findAndReplace) {
            SocketTextChannel channel = GuildHandler.GetChannel (_channelId.GetValue ()) as SocketTextChannel;
            string [ ] guildMessages = messages.GetValue ().ToArray ();
            string message = guildMessages [ new Random ().Next (0, guildMessages.Length) ];

            for (int i = 0; i < findAndReplace.Length; i += 2)
            {
                message = message.Replace(findAndReplace[i], findAndReplace[i + 1]);
            }

            await MessageControl.SendMessage (channel, message);
        }

        public override void Shutdown() {
            GuildHandler.JoinedGuild -= OnJoinedNewGuild;
            GuildHandler.UserJoined -= OnUserJoinedGuild;
            GuildHandler.UserLeft -= OnUserLeftGuild;
            GuildHandler.UserBanned -= OnUserBannedFromGuild;
            GuildHandler.UserUnbanned -= OnUserUnbannedFromGuild;
            GuildHandler.GuildMemberUpdated -= OnGuildMemberUpdated;
        }
    }
}
