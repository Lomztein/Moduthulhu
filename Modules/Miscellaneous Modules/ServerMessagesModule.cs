using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
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

    public class ServerMessagesModule : PluginBase, IConfigurable<MultiConfig> {

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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            inviteHandler.Intialize();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2) {
            await CheckAndAnnounceNameChange (arg1, arg2);
        }

        private async Task CheckAndAnnounceNameChange(SocketGuildUser before, SocketGuildUser after) {

            if (before == null || after == null)
                return;

            if (before.GetShownName () != after.GetShownName ()) {
                await SendMessage (after.Guild, onUserNameChanged, "[USERNAME]", before.GetShownName (), "[NEWNAME]", after.GetShownName ());
            }
        }

        private async Task OnUserUnbannedFromGuild(SocketUser user, SocketGuild guild) {
            await SendMessage (guild, onUserUnbannedFromGuild, "[USERNAME]", user.GetShownName ());
        }

        private async Task OnUserLeftGuild(SocketGuildUser user) {
            await SendMessage (user.Guild, onUserLeftGuild, "[USERNAME]", user.GetShownName ());
            await KickAgesomeIfLomzBannedOrKickedAsync (user, user.Guild, false);
        }

        private async Task OnUserBannedFromGuild(SocketUser user, SocketGuild guild) {
            await SendMessage (guild, onUserBannedFromGuild, "[USERNAME]", user.GetShownName ());
            await KickAgesomeIfLomzBannedOrKickedAsync (user, guild, true);
        }

        private async Task KickAgesomeIfLomzBannedOrKickedAsync (SocketUser user, SocketGuild guild, bool ban) {

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

        private async Task OnUserJoinedGuild(SocketGuildUser user) {
            await OnUserJoinedGuildAsync (user);
        }

        private async Task OnUserJoinedGuildAsync (SocketGuildUser user) {
            RestInviteMetadata invite = await inviteHandler.FindInviter (user.Guild);
            if (invite == null)
                await SendMessage (user.Guild, onUserJoinedGuild, "[USERNAME]", user.GetShownName ());
            else
                await SendMessage (user.Guild, onUserJoinedGuildByInvite, "[USERNAME]", user.GetShownName (), "[INVITERNAME]", invite.Inviter.GetShownName ());
        }

        private async Task OnJoinedNewGuild(SocketGuild guild) {
            await SendMessage (guild, onJoinedNewGuild, "[BOTNAME]", ParentShard.Client.CurrentUser.GetShownName ());
            await inviteHandler.UpdateData (null, guild);
        }

        private async Task SendMessage (SocketGuild guild, MultiEntry<string[], SocketGuild> messages, params string[] findAndReplace) {
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
