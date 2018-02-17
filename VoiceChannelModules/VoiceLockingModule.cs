using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.ModularDiscordBot.Core.Configuration;
using Lomztein.ModularDiscordBot.Core.Extensions;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Lomztein.ModularDiscordBot.Modules.CommandRoot;
using Lomztein.ModularDiscordBot.Modules.Voice.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Voice
{
    public class VoiceLockingModule : ModuleBase, IConfigurable {

        public override string Name => "Voice Locking";
        public override string Description => "Allows people to lock voice channels.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override string [ ] RequiredModules => new string [ ] { "Lomztein_Command Root" };

        private MultiEntry<List<ulong>> nonLockableChannels;
        private MultiEntry<ulong> moveToChannel;

        private Dictionary<ulong, Lock> lockedChannels = new Dictionary<ulong, Lock> ();

        private MultiConfig config;

        private VoiceLockingSet lockingCommandSet = new VoiceLockingSet ();
        private VoiceLockingSet lockingCommandSet2 = new VoiceLockingSet ();

        public void Configure() {
            config = new MultiConfig (this.CompactizeName ());
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();

            nonLockableChannels = config.GetEntries (guilds, "NonLockableChannels", new List<ulong> () { 0 });
            moveToChannel = config.GetEntries (guilds, "MoveToChannel", guilds.Select (x => {
                if (x.AFKChannel != null)
                    return x.AFKChannel.Id;
                return (ulong)0;
            }));

            config.Save ();
        }

        public Config GetConfiguration() {
            throw new NotImplementedException ();
        }

        public override void Initialize() {
            ParentBotClient.discordClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        public override void PostInitialize() {
            var root = ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot;

            lockingCommandSet.parentModule = this;
            root.AddCommands (lockingCommandSet);
            root.AddCommands (lockingCommandSet2);
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState prevState, SocketVoiceState newState) {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser == null || newState.VoiceChannel == null)
                return; // Return instantly if not in a channel, or not in a guild.

            if (!IsUserAllowed (guildUser, newState.VoiceChannel)) {
                await KickUserToPrison (guildUser);
            }

            if (prevState.VoiceChannel != null) {
                if (prevState.VoiceChannel.Users.Count () == 0)
                    UnlockChannel (prevState.VoiceChannel);
            }
        }

        private async Task KickUserToPrison (SocketGuildUser user) {
            SocketVoiceChannel prison = ParentBotClient.GetChannel (moveToChannel.GetEntry (user.Guild)) as SocketVoiceChannel;
            await user.ModifyAsync (x => x.Channel = prison);
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
            var root = ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot;
            root.RemoveCommands (lockingCommandSet);
        }

        public bool IsChannelLocked (SocketVoiceChannel channel) {
            return lockedChannels.ContainsKey (channel.Id);
        }

        public bool IsUserAllowed (SocketGuildUser user, SocketVoiceChannel channel) {
            if (!lockedChannels.ContainsKey (channel.Id))
                return true;
            return lockedChannels [ channel.Id ].allowedMembers.Contains (user.Id);
        }

        public void LockChannel (SocketVoiceChannel channel, IEnumerable<SocketGuildUser> initialMembers) {
            if (!lockedChannels.ContainsKey (channel.Id))
                lockedChannels.Add (channel.Id, new Lock (channel.Id, initialMembers.Select (x => x.Id).ToList ()));
        }

        public void UnlockChannel (SocketVoiceChannel channel) {
            lockedChannels.Remove (channel.Id);
        }

        public List<SocketGuildUser> GetAllowedMembers (SocketVoiceChannel channel) {
            if (lockedChannels.ContainsKey (channel.Id)) {
                return lockedChannels [ channel.Id ].allowedMembers.Select (x => ParentBotClient.GetUser (channel.Guild.Id, x)).ToList ();
            }
            return null;
        }

        public Lock GetLock (SocketVoiceChannel channel) {
            if (IsChannelLocked (channel))
                return lockedChannels [ channel.Id ];
            return null;
        }

        public class Lock {

            public ulong channelID;
            public List<ulong> allowedMembers;

            public Lock (ulong _id, List<ulong> _initialMembers) {
                channelID = _id;
                allowedMembers = _initialMembers;
            }

            public void AddMember (SocketUser user) {
                allowedMembers.Add (user.Id);
            }

            public void KickMember (SocketUser user) {
                allowedMembers.Remove (user.Id);
            }
        }
    }
}
