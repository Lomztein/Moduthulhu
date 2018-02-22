using Discord.WebSocket;
using Lomztein.ModularDiscordBot.Core.Configuration;
using Lomztein.ModularDiscordBot.Core.Extensions;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;

namespace Lomztein.ModularDiscordBot.Modules.Voice
{
    public class AutoVoiceCreatorModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "Auto Voice Creator";
        public override string Description => "Creates new voice channels when all others are full.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private MultiEntry<List<ulong>> defaultChannels; // These are the channels that should never be deleted.
        private MultiEntry<List<string>> newVoiceNames; // Would be more fitting as a queue, but a list is easier to work with in this case.
        private MultiEntry<int> desiredFreeChannels; // However many channels you'd want free at any given time.
        private MultiEntry<List<ulong>> ignoreChannels; // Whichever channels you want this module to completely ignore, such as AFK channels.

        private Dictionary<ulong, List<string>> nameQueue; // This isn't for config, but instead for keeping track of which names have been used.
        private Dictionary<ulong, List<ulong>> temporaryChannels; // This isn't for config, but instead for keeping track of the active channels.

        public void Configure() {
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();

            defaultChannels = Configuration.GetEntries (guilds, "DefaultVoiceChannels", guilds.Select (x => x.VoiceChannels.Select (y => y.Id).ToList ()));
            newVoiceNames = Configuration.GetEntries (guilds, "NewVoiceNames", new List<string> () { "General 1", "General 2" });
            desiredFreeChannels = Configuration.GetEntries (guilds, "DesiredFreeChannels", 1);

            ignoreChannels = Configuration.GetEntries (guilds, "IgnoreChannels", guilds.Select (x => {
                if (x.AFKChannel != null)
                    return x.AFKChannel.Id;
                return (ulong)0;
                }).ToList ());

            nameQueue = new Dictionary<ulong, List<string>> ();
            temporaryChannels = new Dictionary<ulong, List<ulong>> ();

            foreach (var value in newVoiceNames.values) {
                nameQueue.Add (value.Key, value.Value);
                temporaryChannels.Add (value.Key, new List<ulong> ());
            }
        }

        public override void Initialize() {
            ParentBotClient.discordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;
            ParentBotClient.discordClient.ChannelCreated += OnChannelCreated;
            ParentBotClient.discordClient.ChannelDestroyed += OnChannelDeleted;
        }

        private Task OnChannelCreated(SocketChannel channel) {
            if (channel is SocketVoiceChannel) {
                SocketVoiceChannel voiceChannel = channel as SocketVoiceChannel;

                if (!temporaryChannels[voiceChannel.Guild.Id].Contains (channel.Id)) {
                    defaultChannels.values [ voiceChannel.Guild.Id ].Add (channel.Id);
                    Configuration.SetEntry (voiceChannel.Guild.Id, "DefaultVoiceChannels", defaultChannels.GetEntry (voiceChannel.Guild), true);
                }
            }

            return Task.CompletedTask;
        }

        private Task OnChannelDeleted (SocketChannel channel) {
            if (channel is SocketVoiceChannel) {
                SocketVoiceChannel voiceChannel = channel as SocketVoiceChannel;

                if (temporaryChannels [ voiceChannel.Guild.Id ].Contains (channel.Id)) {
                    temporaryChannels[ voiceChannel.Guild.Id ].Remove (channel.Id);
                }

                if (!temporaryChannels [ voiceChannel.Guild.Id ].Contains (channel.Id)) {
                    defaultChannels.values [ voiceChannel.Guild.Id ].Remove (channel.Id);
                    Configuration.SetEntry (voiceChannel.Guild.Id, "DefaultVoiceChannels", defaultChannels.GetEntry (voiceChannel.Guild), true);
                }
            }

            return Task.CompletedTask;
        }

        private async Task UserVoiceStateUpdated (SocketUser user, SocketVoiceState prevState, SocketVoiceState curState) {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser == null)
                return; // Break off instantly if this is in a private DM. Can you even call bots directly?

            List<SocketVoiceChannel> voiceChannels = guildUser.Guild.VoiceChannels.ToList ();

            int freeChannels = 0;
            int desiredFree = desiredFreeChannels.GetEntry (guildUser.Guild);
            List<ulong> toIgnore = ignoreChannels.GetEntry (guildUser.Guild);

            List<string> names = nameQueue [ guildUser.Guild.Id ];

            foreach (SocketVoiceChannel channel in voiceChannels) {
                if (!toIgnore.Contains (channel.Id) && channel.Users.Count == 0)
                    freeChannels++;
            }

            if (freeChannels < desiredFree) {
                string selectedName = names.First ();
                names.Remove (selectedName); // Shuffle dat shiznat.
                names.Add (selectedName);

                await CreateNewChannel (guildUser.Guild, selectedName);
            } else {
                SocketVoiceChannel toDelete = ParentBotClient.GetChannel (guildUser.Guild.Id, temporaryChannels [ guildUser.Guild.Id ].Last ()) as SocketVoiceChannel;
                await DeleteChannel (toDelete);
            }
        }

        private async Task<RestVoiceChannel> CreateNewChannel (SocketGuild guild, string channelName) {
            var channel = await guild.CreateVoiceChannelAsync (channelName);
            temporaryChannels [ guild.Id ].Add (channel.Id);
            return channel;
        }

        private async Task DeleteChannel (SocketVoiceChannel channel) {
            await channel.DeleteAsync ();
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.UserVoiceStateUpdated -= UserVoiceStateUpdated;
            ParentBotClient.discordClient.ChannelCreated -= OnChannelCreated;
            ParentBotClient.discordClient.ChannelDestroyed -= OnChannelDeleted;
        }
    }
}
