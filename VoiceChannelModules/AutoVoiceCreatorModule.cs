using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using Lomztein.Moduthulhu.Modules.Misc.Shipping;

namespace Lomztein.Moduthulhu.Modules.Voice
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
        private MultiEntry<ulong> newChannelCategoryID; // The catagory that new channels are placed in, 0 if no catagory.

        private Dictionary<ulong, List<string>> nameQueue; // This isn't for config, but instead for keeping track of which names have been used.
        private Dictionary<ulong, List<ulong>> temporaryChannels; // This isn't for config, but instead for keeping track of the active channels.

        public void Configure() {
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();

            defaultChannels = Configuration.GetEntries (guilds, "DefaultVoiceChannels", guilds.Select (x => x.VoiceChannels.Select (y => y.Id).ToList ()));
            newVoiceNames = Configuration.GetEntries (guilds, "NewVoiceNames", new List<string> () { "General 1", "General 2" });
            desiredFreeChannels = Configuration.GetEntries (guilds, "DesiredFreeChannels", 1);

            ignoreChannels = Configuration.GetEntries (guilds, "IgnoreChannels", guilds.Select (x => x.AFKChannel.ZeroIfNull ()).ToList ());
            newChannelCategoryID = Configuration.GetEntries (guilds, "NewChannelCategory", (ulong)0);

            nameQueue = new Dictionary<ulong, List<string>> ();
            temporaryChannels = new Dictionary<ulong, List<ulong>> ();

            foreach (var value in newVoiceNames.values) {
                nameQueue.Add (value.Key, value.Value);
                temporaryChannels.Add (value.Key, new List<ulong> ());
            }

            foreach (SocketGuild guild in guilds) {
                var nonCachedChannels = guild.VoiceChannels.Where (x => !defaultChannels.GetEntry (guild).Contains (x.Id));
                temporaryChannels [ guild.Id ] = nonCachedChannels.Select (x => x.Id).ToList ();
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

        private Task UserVoiceStateUpdated (SocketUser user, SocketVoiceState prevState, SocketVoiceState curState) {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser == null)
                return Task.CompletedTask; // Break off instantly if this is in a private DM. Can you even call bots directly?

            CheckAndModifyChannelCount (guildUser);
            return Task.CompletedTask;
        }

        // Have to put it in a seperate async void function, so it doesn't block the event. Async root?
        private async void CheckAndModifyChannelCount (SocketGuildUser user) {

            List<SocketVoiceChannel> voiceChannels = user.Guild.VoiceChannels.ToList ();

            int freeChannels = 0;
            int desiredFree = desiredFreeChannels.GetEntry (user.Guild);
            List<ulong> toIgnore = ignoreChannels.GetEntry (user.Guild);

            List<string> names = nameQueue [ user.Guild.Id ];

            foreach (SocketVoiceChannel channel in voiceChannels) {
                if (!toIgnore.Contains (channel.Id) && channel.Users.Count == 0)
                    freeChannels++;
            }

            if (freeChannels < desiredFree) {
                string selectedName = names.First ();
                names.Remove (selectedName); // Shuffle dat shiznat.
                names.Add (selectedName); // I don't know why this is here and I'm too afraid to remove it.

                await CreateNewChannel (user.Guild, selectedName);
            } else if (freeChannels > desiredFree) {
                if (FindEmptyTemporaryChannel (user.Guild) is SocketVoiceChannel toDelete)
                    await DeleteChannel (toDelete); // Man, pattern matching seems to be able to do anything.
            }
        }

        private SocketVoiceChannel FindEmptyTemporaryChannel (SocketGuild guild) {
            var temps = temporaryChannels[guild.Id].Select (x => ParentBotClient.GetChannel (guild.Id, x) as SocketVoiceChannel);
            return temps.LastOrDefault (x => x.Users.Count == 0);
        }

        private async Task<RestVoiceChannel> CreateNewChannel (SocketGuild guild, string channelName) {
            var channel = await guild.CreateVoiceChannelAsync (channelName);
            temporaryChannels [ guild.Id ].Add (channel.Id);

            ulong catagory = newChannelCategoryID.GetEntry (guild);
            if (catagory != 0) {
                await channel.ModifyAsync (x => x.CategoryId = catagory);
            }

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
