using Discord;
using Discord.WebSocket;
using Lomztein.ModularDiscordBot.Core.Bot;
using Lomztein.ModularDiscordBot.Core.Configuration;
using Lomztein.ModularDiscordBot.Core.Extensions;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Voice {
    public class AutoVoiceNameModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Auto Voice Names";
        public override string Description => "Automatically renames voice channels based on games played within.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private MultiEntry<Dictionary<ulong, string>> channelNames;
        private MultiEntry<List<ulong>> toIgnore;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private Dictionary<ulong, string> customNames = new Dictionary<ulong, string> ();

        public override void Initialize() {
            ParentBotClient.discordClient.ChannelCreated += OnChannelCreated;
            ParentBotClient.discordClient.ChannelDestroyed += OnChannelDestroyed;
            ParentBotClient.discordClient.UserVoiceStateUpdated += OnVoiceStateUpdated;
            ParentBotClient.discordClient.GuildMemberUpdated += OnGuildMemberUpdated;
        }

        private Task OnGuildMemberUpdated(SocketGuildUser prev, SocketGuildUser cur) {
            if (cur.VoiceChannel != null)
                UpdateChannel (cur.VoiceChannel);
            return Task.CompletedTask;
        }

        private Task OnVoiceStateUpdated(SocketUser user, SocketVoiceState prev, SocketVoiceState cur) {
            if (prev.VoiceChannel != null)
                UpdateChannel (prev.VoiceChannel);
            if (cur.VoiceChannel != null)
                UpdateChannel (cur.VoiceChannel);
            return Task.CompletedTask;
        }

        private async void UpdateChannel (SocketVoiceChannel channel) {
            string highestGame = "";

            if (channel != null && ParentBotClient.GetChannel (channel.Guild.Id, channel.Id) != null) {

                string name = channelNames.GetEntry (channel.Guild).GetValueOrDefault (channel.Id);

                if (toIgnore.GetEntry (channel.Guild).Contains (channel.Id))
                    return;

                List<SocketGuildUser> users = channel.Users.ToList ();

                Dictionary<string, int> numPlayers = new Dictionary<string, int> ();
                foreach (SocketGuildUser user in users) {

                    if (user.Activity == null)
                        continue;

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

                string [ ] splitVoice = name.Split (';');
                string possibleShorten = splitVoice.Length > 1 ? splitVoice [ 1 ] : splitVoice [ 0 ];

                string gameName = highestGame;
                string newName = gameName != "" ?  possibleShorten + " - " + gameName : splitVoice [ 0 ];

                if (customNames.ContainsKey (channel.Id))
                    newName = possibleShorten + " - " + customNames[channel.Id];

                // Trying to optimize API calls here, just to spare those poor souls at the Discord API HQ stuff
                if (channel.Name != newName) {
                    await channel.ModifyAsync (x => x.Name = newName);
                }
            }
        }

        private Task OnChannelDestroyed(SocketChannel channel) {
            if (channel is SocketVoiceChannel voice) {
                channelNames.values [ voice.Guild.Id ].Remove (voice.Id);
                Configuration.SetEntry (voice.Guild.Id, "ChannelNames", channelNames.GetEntry (voice.Guild), true);
            }
            return Task.CompletedTask;
        }

        private Task OnChannelCreated(SocketChannel channel) {
            if (channel is SocketVoiceChannel voice) {
                channelNames.values [ voice.Guild.Id ].Add (voice.Id, voice.Name);
                Configuration.SetEntry (voice.Guild.Id, "ChannelNames", channelNames.GetEntry (voice.Guild), true);
                UpdateChannel (voice);
            }
            return Task.CompletedTask;
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.ChannelCreated -= OnChannelCreated;
            ParentBotClient.discordClient.ChannelDestroyed -= OnChannelDestroyed;
            ParentBotClient.discordClient.UserVoiceStateUpdated -= OnVoiceStateUpdated;
            ParentBotClient.discordClient.GuildMemberUpdated -= OnGuildMemberUpdated;
        }

        public void Configure() {
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();
            channelNames = Configuration.GetEntries (guilds, "ChannelNames", guilds.Select (x => x.VoiceChannels.ToDictionary (y => y.Id, z => z.Name)));
            toIgnore = Configuration.GetEntries (guilds, "ToIgnore", new List<ulong> ());
        }
    }
}
