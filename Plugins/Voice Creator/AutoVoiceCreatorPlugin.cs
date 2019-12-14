using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;

namespace Lomztein.Moduthulhu.Modules.Voice
{
    [Descriptor ("Lomztein", "Auto Voice Creator", "Makes sure there are always available voice channels by automatically creating new ones.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Voice%20Creator/AutoVoiceCreatorPlugin.cs")]
    public class AutoVoiceCreatorPlugin : PluginBase {

        private CachedValue<List<ulong>> _defaultChannels;
        private CachedValue<List<string>> _newVoiceNames;
        private CachedValue<int> _desiredFreeChannels;
        private CachedValue<List<ulong>> _ignoreChannels;
        private CachedValue<ulong> _newChannelCategory;

        private List<string> _nameQueue; // This isn't for config, but instead for keeping track of which names have been used.
        private List<ulong> _temporaryChannels; // This isn't for config, but instead for keeping track of the active channels.

        public override void Initialize() {
            AssertPermission(Discord.GuildPermission.ManageChannels);

            GuildHandler.UserVoiceStateUpdated += UserVoiceStateUpdated;
            GuildHandler.ChannelCreated += OnChannelCreated;
            GuildHandler.ChannelDestroyed += OnChannelDeleted;

            _defaultChannels = GetConfigCache("DefaultVoiceChannels", x => x.GetGuild().VoiceChannels.Select(y => y.Id).ToList ());
            _newVoiceNames = GetConfigCache("NewVoiceNames", x => new List<string> { "Extra Voice 1", "Extra Voice 2" });
            _desiredFreeChannels = GetConfigCache("DesiredFreeChannels", x => 1);
            _ignoreChannels = GetConfigCache("IgnoreChannels", x => new List<ulong> { x.GetGuild().AFKChannel.ZeroIfNull () });
            _newChannelCategory = GetConfigCache("NewChannelCategory", x => (ulong)x.GetGuild().VoiceChannels.FirstOrDefault()?.CategoryId.GetValueOrDefault ());

            AddConfigInfo("Add Default Channel", "Add default channel", new Action<SocketVoiceChannel>((x) => _defaultChannels.MutateValue(y => y.Add(x.Id))), () => "Added channel to list of default.", "Channel");
            AddConfigInfo("Add Default Channel", "Add default channel", new Action<ulong>((x) => _defaultChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel (x).Id))), () => "Added channel to list of default.", "Channel");
            AddConfigInfo("Add Default Channel", "Add default channel", new Action<string>((x) => _defaultChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id))), () => "Added channel to list of default.", "Channel");
            AddConfigInfo("Add Default Channel", "List default channels", () => "Current default channels are:\n" + string.Join('\n', _defaultChannels.GetValue().Select(x => GuildHandler.GetVoiceChannel(x).Name)));

            AddConfigInfo("Remove Default Channel", "Remove default channel", new Action<SocketVoiceChannel>((x) => _defaultChannels.MutateValue(y => y.Remove(x.Id))), () => "Removed channel from list of default.", "Channel");
            AddConfigInfo("Remove Default Channel", "Remove default channel", new Action<ulong>((x) => _defaultChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel (x).Id))), () => "Removed channel from list of default.", "Channel");
            AddConfigInfo("Remove Default Channel", "Remove default channel", new Action<string>((x) => _defaultChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel(x).Id))), () => "Removed channel from list of default.", "Channel");

            AddConfigInfo("Add Voice Name", "Add voice name", new Action<string> (x => _newVoiceNames.MutateValue (y => y.Add (x))), () => "Added new voice name to list of possible options.", "Name");
            AddConfigInfo("Add Voice Name", "List voice names", () => "Current possible extra voice names: " + string.Join('\n', _newVoiceNames.GetValue()));
            AddConfigInfo("Remove Voice Name", "Remove voice name", new Action<string>(x => _newVoiceNames.MutateValue(y => y.Add(x))), () => "Removed voice name from list of possible options.", "Name");

            AddConfigInfo("Set Desired Free Channels", "Set desired amount", () => $"Set desired amount to {_desiredFreeChannels.GetValue()}");
            AddConfigInfo("Set Desired Free Channels", "Show desired amount", new Action<int> ((x) => _desiredFreeChannels.SetValue (x)), () => $"Current desired amount is {_desiredFreeChannels.GetValue()}", "Amount");

            AddConfigInfo("Ignore Channel", "Ignore channel", new Action<SocketVoiceChannel>((x) => _ignoreChannels.MutateValue(y => y.Add(x.Id))), () => "Added channel to list of ignored.", "Channel");
            AddConfigInfo("Ignore Channel", "Ignore channel", new Action<ulong>((x) => _ignoreChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel (x).Id))), () => "Added channel to list of default.", "ignored");
            AddConfigInfo("Ignore Channel", "Ignore channel", new Action<string>((x) => _ignoreChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id))), () => "Added channel to list of ignored.", "Channel");

            AddConfigInfo("Unignore Channel", "Unignore channel", new Action<SocketVoiceChannel>((x) => _ignoreChannels.MutateValue(y => y.Remove(x.Id))), () => "Removed channel from list of ignored.", "Channel");
            AddConfigInfo("Unignore Channel", "Unignore channel", new Action<ulong>((x) => _ignoreChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel (x).Id))), () => "Removed channel from list of ignored.", "Channel");
            AddConfigInfo("Unignore Channel", "Unignore channel", new Action<string>((x) => _ignoreChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel(x).Id))), () => "Removed channel from list of ignored.", "Channel");

            AddConfigInfo("Set New Channel Category", "Set category", new Action<SocketCategoryChannel> ((x) => _newChannelCategory.SetValue (x.Id)), () => $"Set category where new channels will be created to {GuildHandler.GetCategoryChannel(_newChannelCategory.GetValue()).Name}", "Channel");
            AddConfigInfo("Set New Channel Category", "Set category", new Action<ulong> ((x) => _newChannelCategory.SetValue (GuildHandler.GetCategoryChannel (x).Id)), () => $"Set category where new channels will be created to {GuildHandler.GetCategoryChannel(_newChannelCategory.GetValue()).Name}", "Channel");
            AddConfigInfo("Set New Channel Category", "Set category", new Action<string> ((x) => _newChannelCategory.SetValue (GuildHandler.GetCategoryChannel (x).Id)), () => $"Set category where new channels will be created to {GuildHandler.GetCategoryChannel(_newChannelCategory.GetValue()).Name}", "Channel");
            AddConfigInfo("Set New Channel Category", "Get category", () => $"New channels will currently be created in category {GuildHandler.GetCategoryChannel(_newChannelCategory.GetValue()).Name}");
        }

        public override void PostInitialize()
        {
            _nameQueue = new List<string>();
            _temporaryChannels = new List<ulong>();

            foreach (var value in _newVoiceNames.GetValue ())
            {
                _nameQueue.Add(value);
            }

            var guild = GuildHandler.GetGuild();
            var nonCachedChannels = guild.VoiceChannels.Where(x => !_defaultChannels.GetValue ().Contains(x.Id));
            _temporaryChannels = nonCachedChannels.Select(x => x.Id).ToList();
        }

        private Task OnChannelCreated(SocketChannel channel) {
            if (channel is SocketVoiceChannel) {
                SocketVoiceChannel voiceChannel = channel as SocketVoiceChannel;

                if (!_temporaryChannels.Contains (channel.Id)) {
                    _defaultChannels.MutateValue (x => x.Add (channel.Id));
                }
            }

            return Task.CompletedTask;
        }

        private Task OnChannelDeleted (SocketChannel channel) {
            if (channel is SocketVoiceChannel) {
                if (!_temporaryChannels.Contains (channel.Id)) {
                    _defaultChannels.MutateValue (x => x.Remove (channel.Id));
                }

                if (_temporaryChannels.Contains(channel.Id))
                {
                    _temporaryChannels.Remove(channel.Id);
                }
            }

            return Task.CompletedTask;
        }

        private async Task UserVoiceStateUpdated (SocketUser user, SocketVoiceState prevState, SocketVoiceState curState) {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser == null)
            {
                return; // Break off instantly if this is in a private DM. Can you even call bots directly?
            }

            await CheckAndModifyChannelCount (guildUser);
        }

        // Have to put it in a seperate async void function, so it doesn't block the event. Async root?
        private async Task CheckAndModifyChannelCount (SocketGuildUser user) {

            List<SocketVoiceChannel> voiceChannels = user.Guild.VoiceChannels.ToList ();

            int freeChannels = 0;
            int desiredFree = _desiredFreeChannels.GetValue ();
            List<ulong> toIgnore = _ignoreChannels.GetValue ();

            List<string> names = _nameQueue;

            foreach (SocketVoiceChannel channel in voiceChannels) {
                if (!toIgnore.Contains (channel.Id) && channel.Users.Count == 0)
                {
                    freeChannels++;
                }
            }

            if (freeChannels < desiredFree) {
                string selectedName = names.First ();
                names.Remove (selectedName); // Shuffle dat shiznat.
                names.Add (selectedName); // I don't know why this is here and I'm too afraid to remove it.

                await CreateNewChannel (user.Guild, selectedName);
            } else if (freeChannels > desiredFree) {
                if (FindEmptyTemporaryChannel () is SocketVoiceChannel toDelete)
                {
                    await DeleteChannel(toDelete); // Man, pattern matching seems to be able to do anything.
                }
            }
        }

        private SocketVoiceChannel FindEmptyTemporaryChannel () {
            var temps = _temporaryChannels.Select (x => GuildHandler.GetChannel (x) as SocketVoiceChannel);
            return temps.LastOrDefault (x => x.Users.Count == 0);
        }

        private async Task<RestVoiceChannel> CreateNewChannel (SocketGuild guild, string channelName) {
            var channel = await guild.CreateVoiceChannelAsync (channelName);
            _temporaryChannels.Add (channel.Id);

            ulong catagory = _newChannelCategory.GetValue ();
            if (catagory != 0) {
                await channel.ModifyAsync (x => x.CategoryId = catagory);
            }

            return channel;
        }

        private static async Task DeleteChannel (SocketVoiceChannel channel) {
            await channel.DeleteAsync ();
        }

        public override void Shutdown() {
            GuildHandler.UserVoiceStateUpdated -= UserVoiceStateUpdated;
            GuildHandler.ChannelCreated -= OnChannelCreated;
            GuildHandler.ChannelDestroyed -= OnChannelDeleted;
        }
    }
}
