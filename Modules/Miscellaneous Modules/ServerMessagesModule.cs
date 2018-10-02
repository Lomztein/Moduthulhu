using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Configuration;
using System.Collections.Generic;
using Lomztein.Moduthulhu.Core.Bot.Messaging;
using Discord.Rest;
using System.Linq;
using Lomztein.Moduthulhu.Modules.Misc.Shipping;
using Discord;

namespace Lomztein.Moduthulhu.Modules.ServerMessages {

    public class ServerMessagesModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Server Messages";
        public override string Description => "Sends a variety of messages on certain events.";
        public override string Author => "Lomztein";
        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        [AutoConfig] private MultiEntry<ulong, SocketGuild> channelIDs = new MultiEntry<ulong, SocketGuild> (x => x.TextChannels.FirstOrDefault ().ZeroIfNull (), "AnnounceChannelID", true);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onJoinedNewGuild = new MultiEntry<string[], SocketGuild> (x => new string[] { "**[BOTNAME]** here, I've arrived to haunt you all with crashes and bugs!" }, "OnJoinedNewGuild", false);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onUserJoinedGuild = new MultiEntry<string[], SocketGuild> (x => new string[] { "**[USERNAME]** has joined this server!" }, "OnUserJoinedGuild", false);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onUserJoinedGuildByInvite = new MultiEntry<string[], SocketGuild> (x => new string[] { "**[USERNAME]** has joined this server through the help of **[INVITERNAME]**!" }, "OnUserJoinedGuildByInvite", false);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onUserLeftGuild = new MultiEntry<string[], SocketGuild> (x => new string[] { "**[USERNAME]** has left this server. :C" }, "OnUserLeftGuild", false);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onUserBannedFromGuild = new MultiEntry<string[], SocketGuild> (x => new string[] { "**[USERNAME]** has been banned from this server." }, "OnUserBannedFromGuild", false);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onUserUnbannedFromGuild = new MultiEntry<string[], SocketGuild> (x => new string[] { "**[USERNAME]** has been unbanned from this server!" }, "OnUserUnbannedFromGuild", false);
        [AutoConfig] private MultiEntry<string [ ], SocketGuild> onUserNameChanged = new MultiEntry<string[], SocketGuild>(x => new string[] { "**[USERNAME] changed their name to **[NEWNAME]**!" }, "OnUserChangedName", false);
        [AutoConfig] private MultiEntry<bool, SocketGuild> KickAgesome1OnLomzLeftOrBanned = new MultiEntry<bool, SocketGuild> (x => true, "KickAgesomeOnLomzBanOrLeft", false); 

        private InviteHandler inviteHandler;

        public override void Initialize() {
            ParentShard.JoinedGuild += OnJoinedNewGuild;
            ParentShard.UserJoined += OnUserJoinedGuild;
            ParentShard.UserLeft += OnUserLeftGuild;
            ParentShard.UserBanned += OnUserBannedFromGuild;
            ParentShard.UserUnbanned += OnUserUnbannedFromGuild;
            ParentShard.GuildMemberUpdated += OnGuildMemberUpdated;

            inviteHandler = new InviteHandler (ParentShard);
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

        private Task OnUserUnbannedFromGuild(SocketUser user, SocketGuild guild) {
            SendMessage (guild, onUserUnbannedFromGuild, "[USERNAME]", user.GetShownName ());
            return Task.CompletedTask;
        }

        private Task OnUserLeftGuild(SocketGuildUser user) {
            SendMessage (user.Guild, onUserLeftGuild, "[USERNAME]", user.GetShownName ());

            KickAgesomeIfLomzBannedOrKickedAsync (user, user.Guild, false);

            return Task.CompletedTask;
        }

        private Task OnUserBannedFromGuild(SocketUser user, SocketGuild guild) {
            SendMessage (guild, onUserBannedFromGuild, "[USERNAME]", user.GetShownName ());

            KickAgesomeIfLomzBannedOrKickedAsync (user, guild, true);

            return Task.CompletedTask;
        }

        private async void KickAgesomeIfLomzBannedOrKickedAsync (SocketUser user, SocketGuild guild, bool ban) {

            if (!KickAgesome1OnLomzLeftOrBanned.GetEntry (guild))
                return;

            // Easter egg, remove at some point in the future, possibly never because it is hilarious.
            try {
                if (user.Id == 93744415301971968) { // It is I, Lomzie!
                    if (guild.Id == 93733172440739840) { // It is the greatest mash of all times.

                        SocketGuildUser agesome1 = guild.GetUser (134335929346293760);
                        IDMChannel channel = await agesome1.GetOrCreateDMChannelAsync ();
                        await channel.SendMessageAsync ("no u");

                        await agesome1.KickAsync ();
                        if (ban) {
                            await guild.AddBanAsync (agesome1, 0, "no u");
                        }

                    }
                }
            } catch (Exception) { }
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
            SendMessage (guild, onJoinedNewGuild, "[BOTNAME]", ParentShard.Client.CurrentUser.GetShownName ());
            inviteHandler.UpdateData (null, guild);
            return Task.CompletedTask;
        }

        private async void SendMessage (SocketGuild guild, MultiEntry<string[], SocketGuild> messages, params string[] findAndReplace) {
            if (!this.IsConfigured (guild.Id))
                return;

            SocketTextChannel channel = ParentShard.GetChannel (guild.Id, channelIDs.GetEntry (guild)) as SocketTextChannel;
            string [ ] guildMessages = messages.GetEntry (guild);
            string message = guildMessages [ new Random ().Next (0, guildMessages.Length) ];

            for (int i = 0; i < findAndReplace.Length; i += 2)
                message = message.Replace (findAndReplace[i], findAndReplace[i+1]);

            await MessageControl.SendMessage (channel, message);
        }

        public override void Shutdown() {
            ParentShard.JoinedGuild -= OnJoinedNewGuild;
            ParentShard.UserJoined -= OnUserJoinedGuild;
            ParentShard.UserLeft -= OnUserLeftGuild;
            ParentShard.UserBanned -= OnUserBannedFromGuild;
            ParentShard.UserUnbanned -= OnUserUnbannedFromGuild;
            ParentShard.GuildMemberUpdated -= OnGuildMemberUpdated;
        }
    }
}
