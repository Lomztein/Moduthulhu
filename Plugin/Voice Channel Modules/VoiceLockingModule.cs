using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.Voice.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice
{
    [Dependency ("CommandRootModule")]
    public class VoiceLockingModule : PluginBase, IConfigurable<MultiConfig> {

        public override string Name => "Voice Locking";
        public override string Description => "Allows people to lock voice channels.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        [AutoConfig] private MultiEntry<List<ulong>, SocketGuild> nonLockableChannels = new MultiEntry<List<ulong>, SocketGuild> (x => new List<ulong> (), "NonLockableChannels");
        [AutoConfig] private MultiEntry<ulong, SocketGuild> moveToChannel = new MultiEntry<ulong, SocketGuild> (x => x.AFKChannel.ZeroIfNull (), "PrisonChannel");

        private Dictionary<ulong, Lock> lockedChannels = new Dictionary<ulong, Lock> ();

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private VoiceLockingSet lockingCommandSet = new VoiceLockingSet ();

        public override void Initialize() {
            lockingCommandSet.ParentModule = this;
            ParentShard.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            ParentContainer.GetModule<CommandRootModule> ().commandRoot.AddCommands (lockingCommandSet);
        }

        public override void PostInitialize() {
            if (ParentContainer.GetModule<AutoVoiceNameModule>() is AutoVoiceNameModule autoVoiceModule) { // You can do this?
                autoVoiceModule.AddTag (new AutoVoiceNameModule.Tag ("🔒", x => IsChannelLocked (x)));
            }
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState prevState, SocketVoiceState newState) {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser == null)
                return; // Return instantly if not in a guild.

            if (newState.VoiceChannel != null && !IsUserAllowed (guildUser, newState.VoiceChannel)) {
                await KickUserToPrison (guildUser);
            }

            if (prevState.VoiceChannel != null) {
                await CheckLock (prevState.VoiceChannel);
            }
        }

        private async Task KickUserToPrison (SocketGuildUser user) {
            SocketVoiceChannel prison = ParentShard.GetVoiceChannel (user.Guild.Id, moveToChannel.GetEntry (user.Guild));
            await user.ModifyAsync (x => x.Channel = prison);
        }

        public override void Shutdown() {
            ParentShard.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
            var root = ParentContainer.GetModule<CommandRootModule> ().commandRoot;
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

        public async Task LockChannel (SocketVoiceChannel channel, IEnumerable<SocketGuildUser> initialMembers) {
            if (channel == channel.Guild.AFKChannel)
                throw new ArgumentException ("You cannot lock the AFK channel, that would be mean.");

            if (!lockedChannels.ContainsKey (channel.Id))
                lockedChannels.Add (channel.Id, new Lock (channel.Id, initialMembers.Select (x => x.Id).ToList ()));

            await UpdateChannelName (channel);
        }

        public async Task UnlockChannel (SocketVoiceChannel channel) {
            lockedChannels.Remove (channel.Id);
            await UpdateChannelName (channel);
        }

        public List<SocketGuildUser> GetAllowedMembers (SocketVoiceChannel channel) {
            if (lockedChannels.ContainsKey (channel.Id)) {
                return lockedChannels [ channel.Id ].allowedMembers.Select (x => ParentShard.GetUser (channel.Guild.Id, x)).ToList ();
            }
            return null;
        }

        private async Task UpdateChannelName (SocketVoiceChannel channel) {
            if (ParentContainer.GetModule<AutoVoiceNameModule> () is AutoVoiceNameModule autoVoiceModule) {
                await autoVoiceModule.UpdateChannel (channel);
            }
        }

        public Lock GetLock (SocketVoiceChannel channel) {
            if (IsChannelLocked (channel))
                return lockedChannels [ channel.Id ];
            return null;
        }

        private async Task CheckLock (SocketVoiceChannel channel) {
            if (channel.Users.Count == 0)
                await UnlockChannel (channel);
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
