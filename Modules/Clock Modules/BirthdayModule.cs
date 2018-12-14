using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Core.Clock;

namespace Lomztein.Moduthulhu.Modules.Clock.Birthday
{
    public class BirthdayModule : ModuleBase, IConfigurable<MultiConfig> {

        private const string dataFilePath = "Birthdays";

        public override string Name => "Birthdays";
        public override string Description => "Enter your birthday and recieve public gratulations when the date arrives!";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        [AutoConfig] private MultiEntry<string, SocketGuild> announcementMessage = new MultiEntry<string, SocketGuild> (x => "Congratulations to **[USERNAME]**, as today they celebrate their [AGE] birthday!", "AnnouncementMessage");
        [AutoConfig] private MultiEntry<ulong, SocketGuild> announcementChannel = new MultiEntry<ulong, SocketGuild> (x => x.TextChannels.FirstOrDefault (y => y.Name == "general" || y.Name == "main" || y.Name == "chat").ZeroIfNull (), "AnnouncementChannel");

        private Dictionary<ulong, Dictionary<ulong, BirthdayDate>> allBirthdays;

        private BirthdayCommand command;

        private void LoadData() {
            allBirthdays = DataSerialization.DeserializeData<Dictionary<ulong, Dictionary<ulong, BirthdayDate>>> (dataFilePath);
            if (allBirthdays == null)
                allBirthdays = new Dictionary<ulong, Dictionary<ulong, BirthdayDate>> ();
            this.GetClock ().OnHourPassed += (prev, now) => TestBirthdays (now);
        }

        private void SaveData() => DataSerialization.SerializeData (allBirthdays, dataFilePath);

        public override void Initialize() {
            LoadData ();
            command = new BirthdayCommand () { ParentModule = this };
        }

        public void SetBirthday(ulong guildID, ulong userID, DateTime date) {
            if (!allBirthdays.ContainsKey (guildID))
                allBirthdays.Add (guildID, new Dictionary<ulong, BirthdayDate> ());

            if (!allBirthdays [ guildID ].ContainsKey (userID))
                allBirthdays [ guildID ].Add (userID, new BirthdayDate (date));

            SaveData ();
        }

        public override void Shutdown() {
            ParentContainer.GetModule<CommandRootModule> ().RemoveCommands (command);
        }

        private async Task TestBirthdays(DateTime now) {

            foreach (var guild in allBirthdays) {
                foreach (var user in guild.Value) {

                    if (user.Value.IsNow ()) {

                        SocketGuildUser guildUser = ParentShard.GetUser (guild.Key, user.Key);
                        if (guildUser == null)
                            return; // User doesn't exist anymore, may have left the server.

                        SocketTextChannel guildChannel = ParentShard.GetTextChannel (guild.Key, announcementChannel.GetEntry (guildUser.Guild));
                        await AnnounceBirthday (guildChannel, guildUser, user.Value);
                        user.Value.SetLastPassedToNow ();
                        SaveData ();
                    }

                }
            }

            return;
        }

        public async Task AnnounceBirthday(ITextChannel channel, SocketGuildUser user, BirthdayDate date) {
            string age = date.GetAge ().ToString () + date.GetAgeSuffix ();
            string message = announcementMessage.GetEntry (channel.Guild).Replace ("[USERNAME]", user.GetShownName ()).Replace ("[AGE]", age);
            await channel.SendMessageAsync (message);
        }

        public class BirthdayDate {

            public DateTime date;
            public long lastPassedYear;

            public BirthdayDate(DateTime _date) {

                date = _date;

                DateTime dateThisYear = new DateTime (GetNow ().Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second);
                if (GetNow () > dateThisYear) {
                    lastPassedYear = GetNow ().Year;
                }

            }

            public int GetAge() {
                try {
                    return DateTime.MinValue.Add (GetNow () - new DateTime (date.Year, date.Month, date.Day)).Year - DateTime.MinValue.Year;
                } catch (IndexOutOfRangeException exc) {
                    Log.Write (exc);
                    return 0;
                }
            }

            public string GetAgeSuffix() => GetAgeSuffix (GetAge ());

            public string GetAgeSuffix(int age) {

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
                }

                if (age % 10 == 0 || age % 10 > 4)
                    ageSuffix = "'th";

                return ageSuffix;
            }

            public bool IsToday() => (date.Month == GetNow ().Month && date.Day == GetNow ().Day);

            public bool IsNow() {
                DateTime dateThisYear = new DateTime (GetNow ().Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                if (GetNow () > dateThisYear && lastPassedYear != GetNow ().Year) {
                    return true;
                }
                return false;
            }

            public void SetLastPassedToNow() => lastPassedYear = DateTime.Now.Year;

            public virtual DateTime GetNow() => DateTime.Now; // Gotta allow for them unit tests amirite?

        }

        public class BirthdayCommand : ModuleCommand<BirthdayModule> {

            public BirthdayCommand () {
                Name = "birthday";
                Description = "Set your birthday date.";
                Category = StandardCategories.Utility;
            }

            [Overload (typeof (void), "Set your birthday to a specific date.")]
            public Task<Result> Execute (CommandMetadata data, int day, int month, int year) {
                DateTime date = new DateTime (year, month, day, 12, 0, 0);
                ParentModule.SetBirthday (data.Message.GetGuild ().Id, data.Message.Author.Id, date);
                return TaskResult (null, $"Succesfully set birthday date to **{date.ToShortDateString ()}**.");
            }

        }
    }
}
