using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Modules.Voice.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice {

    [Dependency ("Lomztein-Command Root")]
    [Descriptor ("Lomztein", "Auto Voice Names", "Automatically renames voice channels to reflect the games played within.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Voice%20Namer")]
    public class AutoVoiceNamePlugin : PluginBase
    {

        private CachedValue<Dictionary<ulong, string>> _channelNames;
        private CachedValue<List<ulong>> _toIgnore;
        // TODO: Implement option to change name format.

        private CachedValue<ulong> _musicBotId;
        private CachedValue<ulong> _internationalRoleId;

        private readonly Dictionary<ulong, string> _customNames = new Dictionary<ulong, string> ();
        private readonly Dictionary<string, Tag> _tags = new Dictionary<string, Tag> (); // This is a dictionary purely for easier identification of tags.

        private VoiceNameSet _commandSet;

        public override void Initialize() {
            AssertPermission(GuildPermission.ManageChannels);

            _commandSet = new VoiceNameSet { ParentPlugin = this };
            GuildHandler.ChannelCreated += OnChannelCreated;
            GuildHandler.ChannelDestroyed += OnChannelDestroyed;
            GuildHandler.UserVoiceStateUpdated += OnVoiceStateUpdated;
            GuildHandler.GuildMemberUpdated += OnGuildMemberUpdated;
            InitDefaultTags();

            _channelNames = GetConfigCache("ChannelNames", x => x.GetGuild().VoiceChannels.ToDictionary(y => y.Id, z => z.Name));
            _toIgnore = GetConfigCache("ToIgnore", x => new List<ulong> { (x.GetGuild().AFKChannel?.Id).GetValueOrDefault() });
            _musicBotId = GetConfigCache("MusicBotId", x => (ulong)0);
            _internationalRoleId = GetConfigCache("MusicBotId", x => (ulong)0);

            AddConfigInfo("Set Channel Name", "Display channel names", () => "Current channel names:\n" + string.Join('\n', _channelNames.GetValue().Select(x => x.Value).ToArray()));
            AddConfigInfo("Set Channel Name", "Set channel name", new Action<SocketVoiceChannel, string>((x, y) => _channelNames.MutateValue(z => z[x.Id] = y)), () => "Succesfully set channel names.", "Channel", "New name");
            AddConfigInfo("Set Channel Name", "Set channel name", new Action<string, string>((x, y) => _channelNames.MutateValue(z => z[GuildHandler.GetVoiceChannel(x).Id] =  y)), () => "Succesfully set channel names.", "Channel", "New name");

            AddConfigInfo("Dont Name Channel", "Ignore channel", new Action<SocketVoiceChannel>((x) => _toIgnore.MutateValue(y => y.Add(x.Id))), () => "Added channel to list of ignored.", "Channel");
            AddConfigInfo("Dont Name Channel", "Ignore channel", new Action<ulong>((x) => _toIgnore.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel (x).Id))), () => "Added channel to list of default.", "ignored");
            AddConfigInfo("Dont Name Channel", "Ignore channel", new Action<string>((x) => _toIgnore.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id))), () => "Added channel to list of ignored.", "Channel");

            AddConfigInfo("Do Name Channel", "Unignore channel", new Action<SocketVoiceChannel>((x) => _toIgnore.MutateValue(y => y.Remove(x.Id))), () => "Removed channel from list of ignored.", "Channel");
            AddConfigInfo("Do Name Channel", "Unignore channel", new Action<ulong>((x) => _toIgnore.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel (x).Id))), () => "Removed channel from list of ignored.", "Channel");
            AddConfigInfo("Do Name Channel", "Unignore channel", new Action<string>((x) => _toIgnore.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel(x).Id))), () => "Removed channel from list of ignored.", "Channel");

            AddConfigInfo("Set Music Bot", "Set music bot.", new Action<SocketGuildUser>((x) => _musicBotId.SetValue(x.Id)), () => $"Set music bot to be {GuildHandler.GetUser(_musicBotId.GetValue()).GetShownName()}.", "Music Bot");
            AddConfigInfo("Set Music Bot", "Set music bot.", new Action<string>((x) => _musicBotId.SetValue(GuildHandler.GetUser(x).Id)), () => $"Set music bot to be {GuildHandler.GetUser(_musicBotId.GetValue()).GetShownName()}.", "Music Bot");
            AddConfigInfo("Set Music Bot", "Show music bot.", () => GuildHandler.FindUser(_musicBotId.GetValue()) == null ? "Current music bot doesn't exist :(" : "Current music bot is " + GuildHandler.GetUser(_musicBotId.GetValue()).GetShownName());

            AddConfigInfo("Set International Role", "Set role.", new Action<SocketRole>((x) => _internationalRoleId.SetValue(x.Id)), () => $"Set international role to be {GuildHandler.GetRole(_internationalRoleId.GetValue()).Name}.", "Role");
            AddConfigInfo("Set International Role", "Set role.", new Action<string>((x) => _internationalRoleId.SetValue(GuildHandler.GetRole(x).Id)), () => $"Set international role to be {GuildHandler.GetRole(_internationalRoleId.GetValue()).Name}.", "Role Name");
            AddConfigInfo("Set International Role", "Show role.", () => GuildHandler.FindRole(_internationalRoleId.GetValue()) == null ? "Current international role doesn't exist :(" : "Current international role is " + GuildHandler.GetRole(_internationalRoleId.GetValue()).Name);

            SendMessage("Lomztein-Command Root", "AddCommand", _commandSet);
        }

        void InitDefaultTags () {
            AddTag (new Tag ("🎵", x => x.Users.Any (y => y.Id == _musicBotId.GetValue ())));
            AddTag (new Tag ("🔥", x => x.Users.Count (y => y.GuildPermissions.Administrator) >= 3));
            AddTag (new Tag ("📹", x => x.Users.Any (y => y.Activity?.Type == ActivityType.Streaming)));
            AddTag (new Tag ("🌎", x => x.Users.Any (y => y.Roles.Any (z => z.Id == _internationalRoleId.GetValue ()))));
        }

        private async Task OnGuildMemberUpdated(SocketGuildUser user, SocketGuildUser cur) {
            if (cur.VoiceChannel != null)
            {
                await UpdateChannel(cur.VoiceChannel);
            }
        }

        private async Task OnVoiceStateUpdated(SocketUser user, SocketVoiceState prev, SocketVoiceState cur) {
            if (prev.VoiceChannel != null)
            {
                await UpdateChannel(prev.VoiceChannel);
            }
            if (cur.VoiceChannel != null)
            {
                await UpdateChannel(cur.VoiceChannel);
            }
        }

        public async Task UpdateChannel(SocketVoiceChannel channel) {
            string highestGame = "";

            if (channel != null) {

                string name = _channelNames.GetValue ().GetValueOrDefault (channel.Id);

                if (GuildHandler.GetChannel (channel.Id) == null)
                {
                    return;
                }

                if (_toIgnore.GetValue ().Contains (channel.Id))
                {
                    return;
                }

                if (string.IsNullOrEmpty (name)) { // If the channel is unknown, then add it and retry through OnChannelCreated.
                    await OnChannelCreated (channel);
                    return;
                }

                List<SocketGuildUser> users = channel.Users.ToList ();

                Dictionary<string, int> numPlayers = new Dictionary<string, int> ();
                foreach (SocketGuildUser user in users) {

                    if (user.Activity == null)
                    {
                        continue;
                    }

                    if (user.Activity.Type == ActivityType.Playing && user.IsBot == false) {
                        if (numPlayers.ContainsKey (user.Activity.Name)) {
                            numPlayers [ user.Activity.Name ]++;
                        } else {
                            numPlayers.Add (user.Activity.Name, 1);
                        }
                    }

                }

                int highest = int.MinValue;

                for (int i = 0; i < numPlayers.Count; i++) {
                    KeyValuePair<string, int> value = numPlayers.ElementAt (i);

                    if (value.Value > highest) {
                        highest = value.Value;
                        highestGame = value.Key;
                    }
                }

                string [ ] splitVoice = name.Split (':');
                string possibleShorten = splitVoice.Length > 1 ? splitVoice [ 1 ] : splitVoice [ 0 ];

                string tags = GetTags (channel);
                string newName = highestGame != "" ? possibleShorten + " - " + highestGame : splitVoice [ 0 ];
                newName = tags + " " + newName;

                if (channel.Users.Count == 0)
                {
                    _customNames.Remove(channel.Id);
                }
                if (_customNames.ContainsKey (channel.Id))
                {
                    newName = possibleShorten + " - " + _customNames[channel.Id];
                }

                // Trying to optimize API calls here, just to spare those poor souls at the Discord API HQ stuff
                if (channel.Name != newName) {
                    try {
                        await channel.ModifyAsync (x => x.Name = newName);
                    }catch (Exception e) {
                        Core.Log.Exception (e);
                    }
                }
            }
        }

        private Task OnChannelDestroyed(SocketChannel channel) {
            if (channel is SocketVoiceChannel voice) {
                _channelNames.MutateValue (x => x.Remove (voice.Id));
            }
            return Task.CompletedTask;
        }

        private async Task OnChannelCreated(SocketChannel channel) {
            if (channel is SocketVoiceChannel voice) {
                _channelNames.MutateValue (x => x.Add (voice.Id, voice.Name));
                await UpdateChannel (voice);
            }
        }

        public override void Shutdown() {
            GuildHandler.ChannelCreated -= OnChannelCreated;
            GuildHandler.ChannelDestroyed -= OnChannelDestroyed;
            GuildHandler.UserVoiceStateUpdated -= OnVoiceStateUpdated;
            GuildHandler.GuildMemberUpdated -= OnGuildMemberUpdated;
            SendMessage("Lomztein-Command Root", "RemoveCommand", _commandSet);
        }

        public void AddTag (Tag newTag) {
            _tags.Add (newTag.Emoji, newTag);
        }

        public void RemoveTag (Tag tag) {
            if (tag == null)
            {
                return;
            }
            _tags.Remove (tag.Emoji);
        }

        public void RemoveTag (string emoji) {
            RemoveTag (_tags.GetValueOrDefault (emoji));
        }

        public string GetTags (SocketVoiceChannel channel) {
            StringBuilder tagString = new StringBuilder ();
            foreach (var tag in _tags) {
                if (tag.Value.IsActive (channel)) {
                    tagString.Append (tag.Value.Emoji);
                }
            }

            return tagString.ToString ();
        }

        public async Task SetCustomName (SocketVoiceChannel channel, string name) {
            if (!_customNames.ContainsKey (channel.Id))
            {
                _customNames.Add(channel.Id, name);
            }
            else
            {
                _customNames[channel.Id] = name;
            }
            await UpdateChannel (channel);
        }

        public async Task ResetCustomName (SocketVoiceChannel channel) {
            _customNames.Remove (channel.Id);
            await UpdateChannel (channel);
        }

        public class Tag {

            public string Emoji { get; private set; } = string.Empty; // The "graphical" representation of the tag.
            public Func<SocketVoiceChannel, bool> IsActive { get; private set; } // Should return true if the tag is active.

            public Tag (string emoji, Func<SocketVoiceChannel, bool> isActive) {
                Emoji = emoji;
                IsActive = isActive;
            }

        }
    }
}
