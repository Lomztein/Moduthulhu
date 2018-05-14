using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Configuration;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Bot;
using Discord.Rest;
using System.Linq;
using Lomztein.Moduthulhu.Modules.Misc.Shipping;

namespace Lomztein.Moduthulhu.Modules.ServerMessages {

    public class ServerMessagesModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Server Messages";
        public override string Description => "Sends a variety of messages on certain events.";
        public override string Author => "Lomztein";
        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private MultiEntry<ulong> channelIDs;
        private MultiEntry<string [ ]> onJoinedNewGuild;
        private MultiEntry<string [ ]> onUserJoinedGuild;
        private MultiEntry<string [ ]> onUserJoinedGuildByInvite;
        private MultiEntry<string [ ]> onUserLeftGuild;
        private MultiEntry<string [ ]> onUserBannedFromGuild;
        private MultiEntry<string [ ]> onUserUnbannedFromGuild;
        private MultiEntry<string [ ]> onUserNameChanged;

        private InviteHandler inviteHandler;

        public override void Initialize() {
            ParentBotClient.discordClient.JoinedGuild += OnJoinedNewGuild;
            ParentBotClient.discordClient.UserJoined += OnUserJoinedGuild;
            ParentBotClient.discordClient.UserLeft += OnUserLeftGuild;
            ParentBotClient.discordClient.UserBanned += OnUserBannedFromGuild;
            ParentBotClient.discordClient.UserUnbanned += OnUserUnbannedFromGuild;
            ParentBotClient.discordClient.GuildMemberUpdated += OnGuildMemberUpdated;

            inviteHandler = new InviteHandler (ParentBotClient);
        }

        private Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2) {
            CheckAndAnnounceNameChange (arg1, arg2);
            return Task.CompletedTask;
        }

        private void CheckAndAnnounceNameChange(SocketGuildUser before, SocketGuildUser after) {

            if (before == null || after == null)
                return;

            if (before.GetShownName () != after.GetShownName ()) {
                SendMessage (after.Guild, onUserNameChanged, "[USERNAME]", before.GetShownName (), "[NEWNAME]", after.GetShownName ());
            }
        }

        public void Configure() {
            IEnumerable<SocketGuild> guilds = ParentBotClient.discordClient.Guilds;

            channelIDs = Configuration.GetEntries<ulong> (guilds, "ChannelID", 0);
            onJoinedNewGuild = Configuration.GetEntries (guilds, "OnJoinedNewGuild", new string [ ] { "Behold! it is I, **[BOTNAME]**!" });
            onUserJoinedGuild = Configuration.GetEntries (guilds, "OnUserJoinedGuild", new string [ ] { "**[USERNAME]** has joined this server!" });
            onUserJoinedGuildByInvite = Configuration.GetEntries (guilds, "OnUserJoinedGuildByInvite", new string [ ] { "**[USERNAME]** has joined this server by the help of **[INVITERNAME]**!" });
            onUserLeftGuild = Configuration.GetEntries (guilds, "OnUserLeftGuild", new string [ ] { "**[USERNAME]** has left this server. ;-;" });
            onUserBannedFromGuild = Configuration.GetEntries (guilds, "OnUserBannedFromGuild", new string [ ] { "**[USERNAME]** has been banned from this server." });
            onUserUnbannedFromGuild = Configuration.GetEntries (guilds, "OnUserUnbannedFromGuild", new string [ ] { "**[USERNAME]** has been unbanned from this server." });
            onUserNameChanged = Configuration.GetEntries (guilds, "OnUserNameChanged", new string [ ] { "**[USERNAME]** changed name to **[NEWNAME]**" });
        }

        private Task OnUserUnbannedFromGuild(SocketUser user, SocketGuild guild) {
            SendMessage (guild, onUserUnbannedFromGuild, "[USERNAME]", user.GetShownName ());
            return Task.CompletedTask;
        }

        private Task OnUserLeftGuild(SocketGuildUser user) {
            SendMessage (user.Guild, onUserLeftGuild, "[USERNAME]", user.GetShownName ());
            return Task.CompletedTask;
        }

        private Task OnUserBannedFromGuild(SocketUser user, SocketGuild guild) {
            SendMessage (guild, onUserBannedFromGuild, "[USERNAME]", user.GetShownName ());
            return Task.CompletedTask;
        }

        private Task OnUserJoinedGuild(SocketGuildUser user) {
            OnUserJoinedGuildAsync (user);
            return Task.CompletedTask;
        }

        private async void OnUserJoinedGuildAsync (SocketGuildUser user) {
            RestInviteMetadata invite = await inviteHandler.FindInviter (user.Guild);
            if (invite == null)
                SendMessage (user.Guild, onUserJoinedGuild, "[USERNAME]", user.GetShownName ());
            else
                SendMessage (user.Guild, onUserJoinedGuildByInvite, "[USERNAME]", user.GetShownName (), "[INVITERNAME]", invite.Inviter.GetShownName ());
        }

        private Task OnJoinedNewGuild(SocketGuild guild) {
            SendMessage (guild, onJoinedNewGuild, "[BOTNAME]", ParentBotClient.discordClient.CurrentUser.GetShownName ());
            inviteHandler.UpdateData (null, guild);
            return Task.CompletedTask;
        }

        private async void SendMessage (SocketGuild guild, MultiEntry<string[]> messages, params string[] findAndReplace) {
            SocketTextChannel channel = ParentBotClient.GetChannel (guild.Id, channelIDs.GetEntry (guild)) as SocketTextChannel;
            string [ ] guildMessages = messages.GetEntry (guild);
            string message = guildMessages [ new Random ().Next (0, guildMessages.Length) ];

            for (int i = 0; i < findAndReplace.Length; i += 2)
                message = message.Replace (findAndReplace[i], findAndReplace[i+1]);

            await MessageControl.SendMessage (channel, message);
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.JoinedGuild -= OnJoinedNewGuild;
            ParentBotClient.discordClient.UserJoined -= OnUserJoinedGuild;
            ParentBotClient.discordClient.UserLeft -= OnUserLeftGuild;
            ParentBotClient.discordClient.UserBanned -= OnUserBannedFromGuild;
            ParentBotClient.discordClient.UserUnbanned -= OnUserUnbannedFromGuild;
            ParentBotClient.discordClient.GuildMemberUpdated -= OnGuildMemberUpdated;
        }
    }
}
