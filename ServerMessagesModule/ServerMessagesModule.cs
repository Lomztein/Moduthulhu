using Discord.WebSocket;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.ModularDiscordBot.Core.Extensions;
using System;
using System.Threading.Tasks;
using Lomztein.ModularDiscordBot.Core.Configuration;
using System.Collections.Generic;
using Lomztein.ModularDiscordBot.Core.Bot;
using Discord.Rest;

namespace Lomztein.ModularDiscordBot.Modules.ServerMessages {

    public class ServerMessagesModule : ModuleBase, IConfigurable {

        public override string Name => "Server Messages";
        public override string Description => "Sends a variety of messages on certain events.";
        public override string Author => "Lomztein";
        public override bool Multiserver => true;

        public MultiConfig config;

        private MultiEntry<ulong> channelIDs;
        private MultiEntry<string [ ]> onJoinedNewGuild;
        private MultiEntry<string [ ]> onUserJoinedGuild;
        private MultiEntry<string [ ]> onUserJoinedGuildByInvite;
        private MultiEntry<string [ ]> onUserLeftGuild;
        private MultiEntry<string [ ]> onUserBannedFromGuild;
        private MultiEntry<string [ ]> onUserUnbannedFromGuild;

        public Config GetConfiguration () => config;

        private InviteHandler inviteHandler = new InviteHandler ();

        public override void Initialize() {
            ParentBotClient.discordClient.JoinedGuild += OnJoinedNewGuild;
            ParentBotClient.discordClient.UserJoined += OnUserJoinedGuild;
            ParentBotClient.discordClient.UserLeft += OnUserLeftGuild;
            ParentBotClient.discordClient.UserBanned += OnUserBannedFromGuild;
            ParentBotClient.discordClient.UserUnbanned += OnUserUnbannedFromGuild;
        }

        public void Configure() {
            config = new MultiConfig (this.CompactizeName ());
            IEnumerable<SocketGuild> guilds = ParentBotClient.discordClient.Guilds;

            channelIDs = config.GetEntries<ulong> (guilds, "ChannelID", 0);
            onJoinedNewGuild = config.GetEntries (guilds, "OnJoinedNewGuild", new string [ ] { "Behold! it is I, [BOTNAME]!" });
            onUserJoinedGuild = config.GetEntries (guilds, "OnUserJoinedGuild", new string [ ] { "[USERNAME] has joined this server!" });
            onUserJoinedGuildByInvite = config.GetEntries (guilds, "OnUserJoinedGuildByInvite", new string [ ] { "[USERNAME] has joined this server by the help of [INVITERNAME]!" });
            onUserLeftGuild = config.GetEntries (guilds, "OnUserLeftGuild", new string [ ] { "[USERNAME] has left this server. ;-;" });
            onUserBannedFromGuild = config.GetEntries (guilds, "OnUserBannedFromGuild", new string [ ] { "[USERNAME] has been banned from this server." });
            onUserUnbannedFromGuild = config.GetEntries (guilds, "OnUserUnbannedFromGuild", new string [ ] { "[USERNAME] has been unbanned from this server." });

            config.Save ();
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

        private async Task OnUserJoinedGuild(SocketGuildUser user) {
            RestInviteMetadata invite = await inviteHandler.FindInviter (user.Guild);
            if (invite == null)
                SendMessage (user.Guild, onUserJoinedGuild, "[USERNAME]", user.GetShownName ());
            else
                SendMessage (user.Guild, onUserJoinedGuild, "[USERNAME]", user.GetShownName (), "[INVITERNAME]", invite.Inviter.GetShownName ());
        }

        private Task OnJoinedNewGuild(SocketGuild guild) {
            SendMessage (guild, onJoinedNewGuild, "[BOTNAME]", ParentBotClient.discordClient.CurrentUser.GetShownName ());
            return Task.CompletedTask;
        }

        private async void SendMessage (SocketGuild guild, MultiEntry<string[]> messages, params string[] findAndReplace) {

            SocketTextChannel channel = guild.GetTextChannel (channelIDs.GetEntry (guild));
            string [ ] guildMessages = messages.GetEntry (guild);
            string message = guildMessages [ new Random ().Next (0, guildMessages.Length) ];

            for (int i = 0; i < findAndReplace.Length; i += 2)
                message.Replace (findAndReplace[i], findAndReplace[i+1]);

            await MessageControl.SendMessage (channel, message);
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.JoinedGuild -= OnJoinedNewGuild;
            ParentBotClient.discordClient.UserJoined -= OnUserJoinedGuild;
            ParentBotClient.discordClient.UserLeft -= OnUserLeftGuild;
            ParentBotClient.discordClient.UserBanned -= OnUserBannedFromGuild;
            ParentBotClient.discordClient.UserUnbanned -= OnUserUnbannedFromGuild;
        }
    }
}
