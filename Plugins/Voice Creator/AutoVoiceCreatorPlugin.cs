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

            _newVoiceNames.OnModified += NewVoiceName_OnModified;

            AddConfigInfo<SocketVoiceChannel>("Add Default Channel", "Add default channel", x => _defaultChannels.MutateValue(y => y.Add(x.Id)), (success, x) => $"Added channel '{x.Name}' to list of default.", "Channel");
            AddConfigInfo<ulong>("Add Default Channel", "Add default channel", x => _defaultChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id)), (success, x) => $"Added channel '{GuildHandler.GetVoiceChannel(x).Name}' to list of default.", "Channel");
            AddConfigInfo<string>("Add Default Channel", "Add default channel", x => _defaultChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id)), (success, x) =>  $"Added channel '{GuildHandler.GetVoiceChannel(x).Name}' to list of default.", "Channel");
            AddConfigInfo("Add Default Channel", "List default channels", () => "Current default channels are:\n" + string.Join('\n', _defaultChannels.GetValue().Select(x => GuildHandler.GetVoiceChannel(x).Name)));

            AddConfigInfo<SocketVoiceChannel>("Remove Default Channel", "Remove default channel", x => _defaultChannels.MutateValue(y => y.Remove(x.Id)), (success, x) => $"Removed channel '{x.Name}' from list of default.", "Channel");
            AddConfigInfo<ulong>("Remove Default Channel", "Remove default channel", x => _defaultChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel (x).Id)), (success, x) => $"Removed channel '{GuildHandler.GetVoiceChannel(x).Name}' from list of default.", "Channel");
            AddConfigInfo<string>("Remove Default Channel", "Remove default channel", x => _defaultChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel(x).Id)), (success, x) => $"Removed channel '{GuildHandler.GetVoiceChannel(x).Name}' from list of default.", "Channel");

            AddConfigInfo<string>("Add Voice Name", "Add voice name", x => _newVoiceNames.MutateValue (y => y.Add (x)), (success, x) => $"Added '{x}' name to list of possible options.", "Name");
            AddConfigInfo("Add Voice Name", "List voice names", () => "Current possible extra voice names:\n " + string.Join('\n', _newVoiceNames.GetValue()));
            AddConfigInfo<string>("Remove Voice Name", "Remove voice name", x => _newVoiceNames.MutateValue(y => y.Remove(x)), (success, x) => $"Removed '{x}' name from list of possible options.", "Name");

            AddConfigInfo<int>("Set Desired Free Channels", "Set desired amount", x => _desiredFreeChannels.SetValue(x), (success, x) => $"Set desired amount to {x}", "Amount");
            AddConfigInfo("Set Desired Free Channels", "Show desired amount", () => $"Current desired amount is {_desiredFreeChannels.GetValue ()}");

            AddConfigInfo<SocketVoiceChannel>("Ignore Channel", "Ignore channel", x => _ignoreChannels.MutateValue(y => y.Add(x.Id)), (success, x) => $"Added channel '{x.Name}' to list of ignored.", "Channel");
            AddConfigInfo<ulong>("Ignore Channel", "Ignore channel", x => _ignoreChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel (x).Id)), (success, x) => $"Added channel '{GuildHandler.GetVoiceChannel(x).Name}' to list of default.", "ignored");
            AddConfigInfo<string>("Ignore Channel", "Ignore channel", x => _ignoreChannels.MutateValue(y => y.Add(GuildHandler.GetVoiceChannel(x).Id)), (success, x) => $"Added channel '{GuildHandler.GetVoiceChannel(x).Name}' to list of ignored.", "Channel");

            AddConfigInfo<SocketVoiceChannel>("Unignore Channel", "Unignore channel", x => _ignoreChannels.MutateValue(y => y.Remove(x.Id)), (success, x) => $"Removed channel '{x.Name}' from list of ignored.", "Channel");
            AddConfigInfo<ulong>("Unignore Channel", "Unignore channel", x => _ignoreChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel (x).Id)), (success, x) => $"Removed channel '{GuildHandler.GetVoiceChannel(x).Name}' from list of ignored.", "Channel");
            AddConfigInfo<string>("Unignore Channel", "Unignore channel", x => _ignoreChannels.MutateValue(y => y.Remove(GuildHandler.GetVoiceChannel(x).Id)), (success, x) => $"Removed channel '{GuildHandler.GetVoiceChannel(x).Name}' from list of ignored.", "Channel");

            AddConfigInfo<SocketCategoryChannel>("Set New Channel Category", "Set category", x => _newChannelCategory.SetValue (x.Id), (success, x) => $"Set category where new channels will be created to {x.Name}", "Channel");
            AddConfigInfo<ulong>("Set New Channel Category", "Set category", x => _newChannelCategory.SetValue (GuildHandler.GetCategoryChannel (x).Id), (success, x) => $"Set category where new channels will be created to {GuildHandler.GetCategoryChannel(x).Name}", "Channel");
            AddConfigInfo<string>("Set New Channel Category", "Set category", x => _newChannelCategory.SetValue (GuildHandler.GetCategoryChannel (x).Id), (success, x) => $"Set category where new channels will be created to {GuildHandler.GetCategoryChannel(x).Name}", "Channel");
            AddConfigInfo("Set New Channel Category", "Get category", () => $"New channels will currently be created in category {GuildHandler.GetCategoryChannel(_newChannelCategory.GetValue()).Name}");
        }

        private void NewVoiceName_OnModified(List<string> arg1, List<string> arg2)
        {
            ResetNameQueue();
        }

        public override void PostInitialize()
        {
            _temporaryChannels = new List<ulong>();
            ResetNameQueue();

            var guild = GuildHandler.GetGuild();
            var nonCachedChannels = guild.VoiceChannels.Where(x => !_defaultChannels.GetValue ().Contains(x.Id));
            _temporaryChannels = nonCachedChannels.Select(x => x.Id).ToList();

            AddGeneralFeaturesStateAttribute("AutomatedVoiceCreation", "Automatic creation of new voice channels when needed.");
        }

        private void ResetNameQueue ()
        {
            _nameQueue = new List<string>();

            foreach (var value in _newVoiceNames.GetValue())
            {
                _nameQueue.Add(value);
            }
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

            DisablePluginIfPermissionMissing(Discord.GuildPermission.ManageChannels, true);
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
