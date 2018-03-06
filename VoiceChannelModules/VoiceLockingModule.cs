using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.Voice.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice
{
    public class VoiceLockingModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Voice Locking";
        public override string Description => "Allows people to lock voice channels.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public override string [ ] RequiredModules => new string [ ] { "Lomztein_Command Root" };
        public override string [ ] RecommendedModules => new string [ ] { "Lomztein_Auto Voice Names" };

        private MultiEntry<List<ulong>> nonLockableChannels;
        private MultiEntry<ulong> moveToChannel;

        private Dictionary<ulong, Lock> lockedChannels = new Dictionary<ulong, Lock> ();

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private VoiceLockingSet lockingCommandSet = new VoiceLockingSet ();

        public void Configure() {
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();

            nonLockableChannels = Configuration.GetEntries (guilds, "NonLockableChannels", new List<ulong> () { 0 });
            moveToChannel = Configuration.GetEntries (guilds, "MoveToChannel", guilds.Select (x => {
                if (x.AFKChannel != null)
                    return x.AFKChannel.Id;
                return (ulong)0;
            }));
        }

        public override void Initialize() {
            ParentBotClient.discordClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot.AddCommands (lockingCommandSet);
        }

        public override void PostInitialize() {
            if (ParentModuleHandler.GetModule<AutoVoiceNameModule>() is AutoVoiceNameModule autoVoiceModule) { // You can do this?
                autoVoiceModule.AddTag (new AutoVoiceNameModule.Tag ("🔒", x => IsChannelLocked (x)));
            }
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState prevState, SocketVoiceState newState) {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser == null)
                return Task.CompletedTask; // Return instantly if not in a guild.

            if (newState.VoiceChannel != null && !IsUserAllowed (guildUser, newState.VoiceChannel)) {
                KickUserToPrison (guildUser);
            }

            if (prevState.VoiceChannel != null) {
                CheckLock (prevState.VoiceChannel);
            }

            return Task.CompletedTask;
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

            UpdateChannelName (channel);
        }

        public void UnlockChannel (SocketVoiceChannel channel) {
            lockedChannels.Remove (channel.Id);
            UpdateChannelName (channel);
        }

        public List<SocketGuildUser> GetAllowedMembers (SocketVoiceChannel channel) {
            if (lockedChannels.ContainsKey (channel.Id)) {
                return lockedChannels [ channel.Id ].allowedMembers.Select (x => ParentBotClient.GetUser (channel.Guild.Id, x)).ToList ();
            }
            return null;
        }

        private void UpdateChannelName (SocketVoiceChannel channel) {
            if (ParentModuleHandler.GetModule<AutoVoiceNameModule> () is AutoVoiceNameModule autoVoiceModule) {
                autoVoiceModule.UpdateChannel (channel);
            }
        }

        public Lock GetLock (SocketVoiceChannel channel) {
            if (IsChannelLocked (channel))
                return lockedChannels [ channel.Id ];
            return null;
        }

        private void CheckLock (SocketVoiceChannel channel) {
            if (channel.Users.Count == 0)
                UnlockChannel (channel);
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
