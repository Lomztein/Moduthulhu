using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Plugins.Standard;
using Lomztein.Moduthulhu.Core.Bot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Plugins.Birthday {

    [Descriptor ("Lomztein", "Birthdays", "Plugin that allows people to enter in their birthdays and have it announced when the date arrives!")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Birthday/BirthdayPlugin.cs")]
    [GDPR (GDPRCompliance.Full)]
    public class BirthdayPlugin : PluginBase {

        private CachedValue<string> _announcementMessage;
        private CachedValue<ulong> _announcementChannel;

        private CachedValue<Dictionary<ulong, BirthdayDate>> _allBirthdays;

        private BirthdayCommand _command;

        public override void Initialize()
        {
            _announcementChannel = GetConfigCache("AnnouncementChannel", x => x.GetGuild().TextChannels.FirstOrDefault(y => y.Name == "general" || y.Name == "main" || y.Name == "chat").ZeroIfNull());
            _announcementMessage = GetConfigCache("AnnouncementMessage", x => "Congratulations to **[USERNAME]** as today they celebrate their [AGE] birthday!");

            AddConfigInfo<string>("Set Birthday Channel", "Set announcement channel.", x => _announcementChannel.SetValue(GuildHandler.GetVoiceChannel (x).Id), x => $"Channel channel set to {GuildHandler.GetTextChannel(x).Mention}.", "Channel Name");
            AddConfigInfo<ulong>("Set Birthday Channel", "Set announcement channel.", x => _announcementChannel.SetValue(GuildHandler.GetVoiceChannel (x).Id), x => $"Channel channel set to {GuildHandler.GetTextChannel(x).Mention}.", "Channel Id");
            AddConfigInfo<SocketTextChannel>("Set Birthday Channel", "Set announcement channel.", x => _announcementChannel.SetValue(x.Id), x => $"Channel channel set to {GuildHandler.GetTextChannel(x)}", "Channel");
            AddConfigInfo("Set Birthday Channel", "Get announcement channel.", () => $"Current announcement channel is {GuildHandler.GetTextChannel(_announcementChannel.GetValue()).Mention}.");

            AddConfigInfo<string>("Set Birthday Message", "Set birthday message.", x => _announcementMessage.SetValue(x), x => $"New announcement message: '{x}'.", "Message");
            AddConfigInfo("Set Birthday Message", "Get birthday message.", () => $"Current announcement message: '{_announcementMessage.GetValue()}'.");

            _command = new BirthdayCommand { ParentPlugin = this };
            _allBirthdays = GetDataCache("Birthdays", x => new Dictionary<ulong, BirthdayDate>());

            GuildHandler.Clock.OnHourPassed += Clock_OnHourPassed;

            SendMessage("Lomztein-Command Root", "AddCommand", _command);

            AddGeneralFeaturesStateAttribute("BirthdayGreetings", "Automatic birthday wishing if desired.");
        }

        private async Task Clock_OnHourPassed(DateTime currentTick, DateTime lastTick)
        {
            await TestBirthdays();
        }

        public void SetBirthday(ulong userID, DateTime date) {

            int lastPassedYear = date.Year;

            DateTime now = DateTime.Now;
            DateTime dateThisYear = new DateTime(now.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            if (now > dateThisYear)
            {
                lastPassedYear = now.Year;
            }

            if (!_allBirthdays.GetValue ().ContainsKey(userID))
            {
                _allBirthdays.MutateValue (x => x.Add (userID, new BirthdayDate (date, lastPassedYear)));
            }
            else
            {
                _allBirthdays.MutateValue (x => x[userID] = new BirthdayDate(date, lastPassedYear));
            }
        }

        public override void Shutdown() {
            SendMessage("Lomztein-CommandRoot", "RemoveCommand", _command);
            ClearConfigInfos();
        }

        private async Task TestBirthdays() {
            bool hasChanged = false;
            foreach (var user in _allBirthdays.GetValue ())
            {
                if (user.Value.IsNow())
                {
                    SocketGuildUser guildUser = GuildHandler.GetUser(user.Key);
                    if (guildUser == null)
                    {
                        return; // User doesn't exist anymore, may have left the server.
                    }

                    SocketTextChannel guildChannel = GuildHandler.GetTextChannel(_announcementChannel.GetValue());
                    await AnnounceBirthday(guildChannel, guildUser, user.Value);
                    user.Value.SetLastPassedToNow();
                    hasChanged = true;
                }
            }
            if (hasChanged)
            {
                _allBirthdays.Store();
            }
        }

        public async Task AnnounceBirthday(ITextChannel channel, SocketGuildUser user, BirthdayDate date) {
            string age = date.GetAge ().ToString () + date.GetAgeSuffix ();
            string message = _announcementMessage.GetValue ().Replace ("[USERNAME]", user.GetShownName ()).Replace ("[AGE]", age);
            await channel.SendMessageAsync (message);
        }

        public override JToken RequestUserData(ulong id)
        {
            if (_allBirthdays.GetValue ().TryGetValue (id, out BirthdayDate date)) {
                return new JObject
                {
                    { "Birthdate", JObject.FromObject (date) }
                };
            }
            return null;
        }

        public override void DeleteUserData(ulong id)
        {
            if (_allBirthdays.GetValue ().ContainsKey (id))
            {
                _allBirthdays.MutateValue(x => x.Remove(id));
            }
        }

        public class BirthdayDate {

            [JsonProperty ("Date")]
            private readonly DateTime _date;
            [JsonProperty ("LastPassedYear")]
            private long _lastPassedYear;

            public BirthdayDate(DateTime _date, long lastPassedYear)
            {
                this._date = _date;
                _lastPassedYear = lastPassedYear;
            }

            public int GetAge() {
                try {
                    return DateTime.MinValue.Add (GetNow () - new DateTime (_date.Year, _date.Month, _date.Day)).Year - DateTime.MinValue.Year;
                } catch (IndexOutOfRangeException exc) {
                    Core.Log.Exception (exc);
                    return 0;
                }
            }

            public string GetAgeSuffix() => GetAgeSuffix (GetAge ());

            public static string GetAgeSuffix(int age) {

                string ageSuffix = "'th";
                switch (age.ToString ().Last ()) {
                    case '1':
                        ageSuffix = "'st";
                        break;
                    case '2':
                        ageSuffix = "'nd";
                        break;
                    case '3':
                        ageSuffix = "'rd";
                        break;
                    default:
                        ageSuffix = "'th";
                        break;
                }

                return ageSuffix;
            }

            public bool IsToday() => (_date.Month == GetNow ().Month && _date.Day == GetNow ().Day);

            public bool IsNow() {
                DateTime dateThisYear = new DateTime (GetNow ().Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second);
                if (GetNow () > dateThisYear && _lastPassedYear != GetNow ().Year) {
                    return true;
                }
                return false;
            }

            public void SetLastPassedToNow() => _lastPassedYear = DateTime.Now.Year;

            public virtual DateTime GetNow() => DateTime.Now; // Gotta allow for them unit tests amirite?

        }

        public class BirthdayCommand : PluginCommand<BirthdayPlugin> {

            public BirthdayCommand () {
                Name = "birthday";
                Description = "Set your birthday date.";
                Category = StandardCategories.Utility;
            }

            [Overload (typeof (void), "Set your birthday to a specific date.")]
            public Task<Result> Execute (CommandMetadata data, int day, int month, int year) {
                Consent.AssertConsent((data.Author as SocketGuildUser).Guild.Id, data.AuthorID);
                DateTime date = new DateTime (year, month, day, 12, 0, 0);
                ParentPlugin.SetBirthday (data.Message.Author.Id, date);
                return TaskResult (null, $"Succesfully set birthday date to **{date.ToShortDateString ()}**.");
            }

        }
    }
}
