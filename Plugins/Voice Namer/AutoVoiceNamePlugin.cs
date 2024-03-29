﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Modules.Voice.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Voice {

    [Dependency ("Moduthulhu-Command Root")]
    [Descriptor ("Lomztein", "Auto Voice Names", "Automatically renames voice channels to reflect the games played within.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/tree/master/Plugins/Voice%20Namer")]
    public class AutoVoiceNamePlugin : PluginBase
    {

        private CachedValue<Dictionary<ulong, string>> _channelNames;
        private CachedValue<List<ulong>> _toIgnore;

        private CachedValue<string> _nameFormat;
        private const string _formatNameStr = "NAME";
        private const string _formatGameStr = "GAME";
        private const string _formatAmountPlayersStr = "PLAYERCOUNT";

        private const string _formatStart = "{";
        private const string _formatEnd = "}";

        private CachedValue<ulong> _musicBotId;
        private CachedValue<ulong> _internationalRoleId;
        private CachedValue<Dictionary<string, string>> _shortenedGameNames;

        private Dictionary<ulong, TimedAction> _resetters = new Dictionary<ulong, TimedAction>();
        private const int _resetterTime = 15 * 60;

        private readonly Dictionary<ulong, string> _customNames = new Dictionary<ulong, string> ();
        private readonly Dictionary<string, Tag> _tags = new Dictionary<string, Tag> (); // This is a dictionary purely for easier identification of tags.
        private readonly Dictionary<ulong, int> _pendingNameChanges = new Dictionary<ulong, int>();

        private VoiceNameSet _commandSet;

        public override void Initialize() {
            AssertPermission(GuildPermission.ManageChannels);

            _commandSet = new VoiceNameSet { ParentPlugin = this };
            GuildHandler.ChannelCreated += OnChannelCreated;
            GuildHandler.ChannelDestroyed += OnChannelDestroyed;
            GuildHandler.UserVoiceStateUpdated += OnVoiceStateUpdated;
            GuildHandler.GuildMemberUpdated += OnGuildMemberUpdated;
            GuildHandler.ChannelUpdated += OnChannelUpdated;
            InitDefaultTags();

            _channelNames = GetConfigCache("ChannelNames", x => x.GetGuild().VoiceChannels.ToDictionary(y => y.Id, z => z.Name));
            _channelNames.Store();

            _toIgnore = GetConfigCache("ToIgnore", x => new List<ulong> { (x.GetGuild().AFKChannel?.Id).GetValueOrDefault() });
            _musicBotId = GetConfigCache("MusicBotId", x => (ulong)0);
            _internationalRoleId = GetConfigCache("MusicBotId", x => (ulong)0);
            _nameFormat = GetConfigCache("NameFormat", x => $"{_formatStart}{_formatNameStr}{_formatEnd} - {_formatStart}{_formatGameStr}{_formatEnd} {_formatStart}({_formatAmountPlayersStr}){_formatEnd}"); // lol
            _shortenedGameNames = GetConfigCache("ShortenedGameNames", x => new Dictionary<string, string>());

            AddConfigInfo("Set Channel Name", "Display channel names", () => "Current channel names:\n" + string.Join('\n', _channelNames.GetValue().Select(x => x.Value).ToArray()));
            AddConfigInfo<SocketVoiceChannel, string>("Set Channel Name", "Set channel name", (x, y) => _channelNames.MutateValue(z => z[x.Id] = y), (success, x, y) => $"Succesfully set channel '{x.Name}' to '{y}'", "Channel", "New name");
            AddConfigInfo<string, string>("Set Channel Name", "Set channel name", (x, y) => _channelNames.MutateValue(z => z[GuildHandler.GetVoiceChannel(x).Id] =  y), (success, x, y) => "Succesfully set channel names.", "Channel", "New name");

            AddConfigInfo<SocketVoiceChannel>("Dont Name Channel", "Ignore channel", x => _toIgnore.MutateValue(y => y.Add(x.Id)), (success, x) => $"Added channel '{x.Name}' to list of ignored channels.", "Channel");
            AddConfigInfo<ulong>("Dont Name Channel", "Ignore channel", x => _toIgnore.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel (x).Id)), (success, x) => $"Added channel '{GuildHandler.GetVoiceChannel (x).Name}' to list of ignored channels.", "ignored");
            AddConfigInfo<string>("Dont Name Channel", "Ignore channel", x => _toIgnore.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id)), (success, x) => $"Added channel '{GuildHandler.GetVoiceChannel(x).Name}' to list of ignored channels.", "Channel");

            AddConfigInfo<SocketVoiceChannel>("Do Name Channel", "Unignore channel", x => _toIgnore.MutateValue(y => y.Remove(x.Id)), (success, x) => $"Removed channel '{x.Name}' from list of ignored.", "Channel");
            AddConfigInfo<ulong>("Do Name Channel", "Unignore channel", x => _toIgnore.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel (x).Id)), (success, x) => $"Removed channel '{GuildHandler.GetChannel(x)}' from list of ignored.", "Channel");
            AddConfigInfo<string>("Do Name Channel", "Unignore channel", x => _toIgnore.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel(x).Id)), (success, x) => $"Removed channel '{GuildHandler.GetChannel(x)}' from list of ignored.", "Channel");

            AddConfigInfo<SocketGuildUser>("Set Music Bot", "Set music bot.", x => _musicBotId.SetValue(x.Id), (success, x) => $"Set music bot to be {x.Id}.", "Music Bot");
            AddConfigInfo<string>("Set Music Bot", "Set music bot.", x => _musicBotId.SetValue(GuildHandler.GetUser(x).Id), (success, x) => $"Set music bot to be {GuildHandler.GetUser(x).GetShownName()}.", "Music Bot");
            AddConfigInfo("Set Music Bot", "Show music bot.", () => GuildHandler.FindUser(_musicBotId.GetValue()) == null ? "Current music bot doesn't exist :(" : "Current music bot is " + GuildHandler.GetUser(_musicBotId.GetValue()).GetShownName());

            AddConfigInfo<SocketRole>("Set International Role", "Set role.", x => _internationalRoleId.SetValue(x.Id), (success, x) => $"Set international role to be {x}.", "Role");
            AddConfigInfo<string>("Set International Role", "Set role.", x => _internationalRoleId.SetValue(GuildHandler.GetRole(x).Id), (success, x) => $"Set international role to be {GuildHandler.GetRole(x).Name}.", "Role Name");
            AddConfigInfo("Set International Role", "Show role.", () => GuildHandler.FindRole(_internationalRoleId.GetValue()) == null ? "Current international role doesn't exist :(" : "Current international role is " + GuildHandler.GetRole(_internationalRoleId.GetValue()).Name);

            AddConfigInfo("Set Name Format", "Set format", () => $"Current format is '{_nameFormat.GetValue()}' which might look like this in practice: '{FormatName(_nameFormat.GetValue(), _formatStart, _formatEnd, "General 1", "Cool Game 3: The Coolest", 5)}'.");
            AddConfigInfo<string>("Set Name Format", "Set format", x => _nameFormat.SetValue (x), (success, x) => $"Set format to '{x}' which might look like this in practice: '{FormatName(x, _formatStart, _formatEnd, "General 1", "Cool Game 3: The Coolest", 5)}'.", "Format");

            AddConfigInfo<string, string>("Shorten Game Name", "Shorten a games name", (x, y) => SetShortenedGameName (x, y), (success, x, y) => $"'{x}' will now be shortened to '{y}'.", "Game", "Name");
            SendMessage("Moduthulhu-Command Root", "AddCommand", _commandSet);

            AddGeneralFeaturesStateAttribute("AutomatedVoiceNames", "Automatically changing voice channel names to reflect games played within.");

            RegisterMessageAction("AddTag", x => AddTag(new Tag((string)x[0], (string)x[1], (Func<SocketVoiceChannel, bool>)x[2])));
            RegisterMessageAction("RemoveTag", x => RemoveTag((string)x[0]));

            SetStateChangeHeaders("Tags", "The following voice channel tags has been added", "The following voice channel tags has been removed", "The following  voice channel tags has been modified");

            RegisterMessageAction("UpdateChannel", x => UpdateChannel(GuildHandler.GetVoiceChannel((ulong)x[0])).ConfigureAwait(false));
        }

        private void AddResetter (ulong id)
        {
            TimedAction action = InvokeTimedAction(() => UpdateChannel(GuildHandler.GetVoiceChannel(id)).ConfigureAwait(false), _resetterTime);
            _resetters.Add(id, action);
        }

        private void RemoveResetter (ulong id)
        {
            if (_resetters.ContainsKey(id))
            {
                _resetters[id].Cancel();
                _resetters.Remove(id);
            }
        }

        private void SetShortenedGameName (string game, string shortened)
        {
            if (!_shortenedGameNames.GetValue ().ContainsKey (game))
            {
                _shortenedGameNames.MutateValue (x => x.Add (game, shortened));
            }
            else
            {
                _shortenedGameNames.MutateValue(x => x[game] = shortened);
            }
        }

        private async Task OnChannelUpdated(SocketChannel before, SocketChannel after)
        {
            if (after is SocketVoiceChannel vc)
            {
                if ((before as SocketVoiceChannel).Name != vc.Name)
                {
                    if (_pendingNameChanges.ContainsKey(vc.Id))
                    {
                        RemovePendingChange(vc.Id);
                    }
                    else if (_channelNames.GetValue().ContainsKey(vc.Id))
                    {
                        _channelNames.MutateValue(x => x[vc.Id] = vc.Name);
                        Core.Log.Debug($"Channel '{vc.Name}' changed by outside source and this has been stored.");
                        await UpdateChannel(vc);
                    }
                    else
                    {
                        _channelNames.MutateValue(x => x.Add(vc.Id, vc.Name));
                        Core.Log.Debug($"Channel '{vc.Name}' changed by outside source, but not already in system? This has been stored.");
                        await UpdateChannel(vc);
                    }
                }

                if (vc.Users.Count == 0)
                {
                    AddResetter(vc.Id);
                }
                else
                {
                    RemoveResetter(vc.Id);
                }
            }
        }

        private string FormatName(string format, string starter, string ender, string name, string game, int playerAmount)
        {
            string result = format;
            string playerAmt = playerAmount == 0 ? "" : playerAmount.ToString(CultureInfo.InvariantCulture);

            result = ReplacePart(result, _formatNameStr, starter, ender, name);
            result = ReplacePart(result, _formatGameStr, starter, ender, game);
            result = ReplacePart(result, _formatAmountPlayersStr, starter, ender, playerAmt);

            return result;
        }

        private string ReplacePart (string input, string part, string starter, string ender, string replacement)
        {
            Regex regex = new Regex($"{starter}.*?{part}.*?{ender}");
            return regex.Replace(input, x => TrimOuter (x.Value.Replace(part, replacement)));
        }

        private string TrimOuter (string input)
        {
            return input.Substring(1, input.Length - 2);
        }

        void InitDefaultTags () {
            AddTag (new Tag ("🎵", "Music Bot is present in channel.", x => x.Users.Any (y => y.Id == _musicBotId.GetValue ())));
            AddTag (new Tag ("🔥", "A trio or more elitist scum is present.", x => x.Users.Count (y => y.GuildPermissions.Administrator) >= 3));
            AddTag (new Tag ("📹", "Someone is streaming in this channel.", x => x.Users.Any (y => y.Activities.Any(z => z.Type == ActivityType.Streaming))));
            AddTag (new Tag ("🌎", "Someone marked 'International' is in this channel.", x => x.Users.Any (y => y.Roles.Any (z => z.Id == _internationalRoleId.GetValue ()))));
        }

        private async Task OnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> user, SocketGuildUser cur) {
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

            DisablePluginIfPermissionMissing(GuildPermission.ManageChannels, true);

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

                    if (user.Activities.Count() == 0)
                    {
                        continue;
                    }

                    int unknowns = 0;
                    if (!user.IsBot)
                    {
                        foreach (var activity in user.Activities)
                        {
                            if (activity.Type == ActivityType.Playing)
                            {
                                string activityName = activity.Name;

                                if (!numPlayers.ContainsKey(activityName))
                                {
                                    numPlayers.Add(activityName, 0);
                                }
                                numPlayers[activityName]++;
                            }
                            else
                            {
                                unknowns++;
                            }
                        }
                    }

                    // Temporary solution to the bot not being able to see game being played, if the user has a custom status.
                    // In this case, if the user is not a bot, it is assumed they are playing all games currently being played by others.
                    for (int i = 0; i < numPlayers.Count; i++)
                    {
                        numPlayers[numPlayers.ElementAt(i).Key] += unknowns;
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

                if (_shortenedGameNames.GetValue ().ContainsKey (highestGame))
                {
                    highestGame = _shortenedGameNames.GetValue()[highestGame];
                }

                string [ ] splitVoice = name.Split (':');
                string possibleShorten = splitVoice.Length > 1 ? splitVoice [ 1 ] : splitVoice [ 0 ];

                string tags = GetTags (channel);
                string newName = splitVoice[0];
                if (!string.IsNullOrWhiteSpace (highestGame))
                {
                    newName = FormatName(_nameFormat.GetValue(), _formatStart, _formatEnd, possibleShorten, highestGame, highest);
                }
                if (!string.IsNullOrWhiteSpace (tags))
                {
                    newName = tags + " " + newName;
                }

                if (channel.Users.Count == 0)
                {
                    _customNames.Remove(channel.Id);
                }
                if (_customNames.ContainsKey (channel.Id))
                {
                    newName = FormatName(_nameFormat.GetValue(), _formatStart, _formatEnd, possibleShorten, _customNames[channel.Id], 0);
                }

                // Trying to optimize API calls here, just to spare those poor souls at the Discord API HQ stuff
                if (channel.Name != newName) {
                    try {
                        AddPendingChange(channel.Id);
                        try
                        {
                            await channel.ModifyAsync(x => x.Name = newName, new RequestOptions() { RetryMode = RetryMode.AlwaysFail, Timeout = 10000 });
                        } catch (RateLimitedException)
                        {
                            Core.Log.Warning(channel.Name + " modification has been ratelimited.");
                            RemovePendingChange(channel.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        RemovePendingChange(channel.Id);
                        Core.Log.Exception (e);
                    }
                }
            }
        }

        private void AddPendingChange (ulong id)
        {
            if (!_pendingNameChanges.ContainsKey(id))
            {
                _pendingNameChanges.Add(id, 0);
                Core.Log.Debug($"Channel with ID '{id}' has had a pending name tracker added.");
            }

            _pendingNameChanges[id]++;
            Core.Log.Debug($"Channel with ID '{id}' pending name changes: {_pendingNameChanges[id]}.");
        }

        private void RemovePendingChange (ulong id)
        {
            if (_pendingNameChanges.ContainsKey(id))
            {
                _pendingNameChanges[id]--;
                Core.Log.Debug($"Channel with ID '{id}' has had a pending name change subtracted.");
                Core.Log.Debug($"Channel with ID '{id}' has {_pendingNameChanges[id]} pending name changes.");
                if (_pendingNameChanges[id] <= 0)
                {
                    _pendingNameChanges.Remove(id);
                    Core.Log.Debug($"Channel with ID '{id}' has had a had their pending name change tracker removed.");
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
            GuildHandler.ChannelUpdated -= OnChannelUpdated;

            SendMessage("Moduthulhu-Command Root", "RemoveCommand", _commandSet);
        }

        public void AddTag (Tag newTag) {
            _tags.Add (newTag.Emoji, newTag);
            AddStateAttribute("Tags", newTag.Emoji, newTag.Emoji + " - " + newTag.Description);
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
            public string Description { get; private set; } = string.Empty;
            public Func<SocketVoiceChannel, bool> IsActive { get; private set; } // Should return true if the tag is active.

            public Tag (string emoji, string desc, Func<SocketVoiceChannel, bool> isActive) {
                Emoji = emoji;
                Description = desc;
                IsActive = isActive;
            }

        }
    }
}
