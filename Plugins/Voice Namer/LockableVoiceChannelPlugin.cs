using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins
{
    [Descriptor ("Lomztein", "Lockable Voice Channels", "Addon plugin for Auto Voice Names that provide a simple locking function for voice channels. Can work without Auto Voice Names enabled, but it is strongly recommended you enable Auto Voice Names.")]
    [GDPR (GDPRCompliance.Partial, "This plugin temporarily stores user ID's locally to keep track of who's allowed in locked channels.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Voice%20Namer")]
    [Dependency ("Moduthulhu-Command Root")]
    public class LockableVoiceChannelPlugin : PluginBase
    {
        private Dictionary<ulong, Lock> _channelLocks;
        private const string _lockEmoji = "🔒";
        private const string _autoVoiceNamePluginName = "Lomztein-Auto Voice Names";

        public override void Initialize()
        {
            throw new NotImplementedException("This plugin is not yet ready to be activated.");

            AssertPermission(Discord.GuildPermission.ManageChannels);
            RegisterMessageFunction("IsLocked", x => _channelLocks.ContainsKey((ulong)x[0]));
            RegisterMessageAction("Lock", x => LockChannel((ulong)x[0], (IEnumerable<ulong>)x[1]));
            RegisterMessageAction("Unlock", x => UnlockChannel((ulong)x[0]));
        }

        public override void PostInitialize()
        {
            SendMessage("Lomztein-Auto Voice Names", "AddTag", _lockEmoji, "This channel is currently locked.", new Func<SocketVoiceChannel, bool> (x => IsLocked (x.Id)));

            if (!GuildHandler.Plugins.IsPluginActive(_autoVoiceNamePluginName))
            {
                foreach (var channel in GuildHandler.GetGuild().VoiceChannels.Where(x => x.Name.StartsWith(_lockEmoji)))
                {
                    LockChannel(channel.Id, channel.Users.Select (x => x.Id));
                }
            }
        }

        public override void Shutdown()
        {
            SendMessage("Lomztein-Auto Voice Names", "RemoveTag", _lockEmoji);
        }

        public bool IsLocked(ulong voiceChannel) => _channelLocks.ContainsKey(voiceChannel);
        public bool HasAccess(ulong user, ulong voiceChannel) => _channelLocks.ContainsKey (voiceChannel) ? _channelLocks[voiceChannel].Contains (user) : true;

        public Lock LockChannel (ulong voiceChannel, IEnumerable<ulong> members)
        {
            if (_channelLocks.ContainsKey (voiceChannel))
            {
                throw new InvalidOperationException("That channel is already locked.");
            }

            var voiceLock = new Lock(voiceChannel, members);
            _channelLocks.Add(voiceChannel, voiceLock);
            UpdateChannel(voiceChannel, IsLocked (voiceChannel));

            voiceLock.OnUserRemoved += VoiceLock_OnUserRemoved;

            return voiceLock;
        }

        private void VoiceLock_OnUserRemoved(ulong channel, ulong user)
        {
            var voiceChannel = GuildHandler.FindVoiceChannel(channel);
            if (voiceChannel != null)
            {
                var voiceUser = voiceChannel.GetUser(user);
                if (voiceUser != null)
                {
                    voiceUser.ModifyAsync(x => x.Channel = null).ConfigureAwait (false);
                }
            }

            UpdateChannel(channel, IsLocked(channel));
        }

        public bool UnlockChannel (ulong voiceChannel)
        {
            if (_channelLocks.ContainsKey (voiceChannel))
            {
                _channelLocks[voiceChannel].OnUserRemoved -= VoiceLock_OnUserRemoved;
                _channelLocks.Remove(voiceChannel);
                return true;
            }
            return false;
        }

        private void UpdateChannel(ulong voiceChannel, bool isLocked)
        {
            if (GuildHandler.Plugins.IsPluginActive (_autoVoiceNamePluginName))
            {
                SendMessage(_autoVoiceNamePluginName, "UpdateChannel", voiceChannel);
            }
            else
            {
                DisablePluginIfPermissionMissing(Discord.GuildPermission.ManageChannels, true);

                SocketVoiceChannel channel = GuildHandler.FindVoiceChannel(voiceChannel);
                if (channel != null)
                {
                    if (isLocked)
                    {
                        if (!channel.Name.StartsWith(_lockEmoji))
                        {
                            channel.ModifyAsync(x => x.Name = _lockEmoji + x.Name).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        if (channel.Name.StartsWith(_lockEmoji))
                        {
                            channel.ModifyAsync(x => x.Name = x.Name.Value.Substring (_lockEmoji.Length)).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        public class Lock
        {
            public readonly ulong ChannelId;
            private readonly List<ulong> _members = new List<ulong>();
            public event Action<ulong, ulong> OnUserRemoved;

            public Lock (ulong id, IEnumerable<ulong> members)
            {
                ChannelId = id;
                _members = members.ToList();
            }

            public void AddMember(ulong member)
            {
                if (!Contains (member))
                {
                    _members.Add(member);
                }
            }

            public void RemoveMember (ulong member)
            {
                if (Contains (member))
                {
                    _members.Remove(member);
                    OnUserRemoved?.Invoke(ChannelId, member);
                }
            }

            public bool Contains(ulong member) => _members.Contains(member);
        }
    }
}
