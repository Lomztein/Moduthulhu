using Lomztein.ModularDiscordBot.Core.Module.Framework;
using Discord.WebSocket;
using Discord;
using System;
using Lomztein.ModularDiscordBot.Core.Configuration;
using Lomztein.ModularDiscordBot.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lomztein.ModularDiscordBot.Core.IO;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.ModularDiscordBot.Core.Bot;

namespace Lomztein.ModularDiscordBot.Modules.Clock.ActivityMonitor
{
    public class UserActivityMonitorModule : ModuleBase, ITickable, IConfigurable<MultiConfig> {

        public override string Name => "User Activity Module";
        public override string Description => "Catagorises people into configurable roles based on their last date of activity. Not recommended, but possible for multiserver.";
        public override string Author => "Lomztein";

        public override string [ ] RequiredModules => new string[] { "Lomztein_Clock Module"};

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private Dictionary<ulong, Dictionary<ulong, DateTime>> userActivity;
        private MultiEntry<ActivityRole[]> activityRoles;

        public override void Shutdown() { }

        public void Configure() {
            IEnumerable<SocketGuild> guilds = ParentBotClient.discordClient.Guilds;
            activityRoles = Configuration.GetEntries (guilds, "ActivityRoles", new ActivityRole [ ] { new ActivityRole (0, 7) });
        }

        private void SaveData () => DataSerialization.SerializeData (userActivity, "UserActivity");
        private void LoadData() {
            userActivity = DataSerialization.DeserializeData<Dictionary<ulong, Dictionary<ulong, DateTime>>> ("UserActivity");
            if (userActivity == null)
                userActivity = new Dictionary<ulong, Dictionary<ulong, DateTime>> ();
        }

        public override void Initialize () {

            LoadData ();

            ParentBotClient.discordClient.MessageReceived += (e) => {
                RecordActivity (e.Author as SocketGuildUser, DateTime.Now, true);
                return Task.CompletedTask;
            };

            ParentBotClient.discordClient.UserVoiceStateUpdated += (user, before, after) => {
                SocketGuildUser afterUser = user as SocketGuildUser;
                if (afterUser?.VoiceChannel != null) {
                    RecordActivity (afterUser, DateTime.Now, true);
                }

                return Task.CompletedTask;
            };

            ParentBotClient.discordClient.UserJoined += (user) => {
                RecordActivity (user, DateTime.Now, true);
                return Task.CompletedTask;
            };
        }

        public override void PostInitialize () {
            ParentModuleHandler.GetModule<ClockModule> ().AddTickable (this);
        }

        public async void RecordActivity(SocketGuildUser user, DateTime time, bool single) {
            SocketGuild guild = user.Guild;

            if (!userActivity.ContainsKey (guild.Id))
                userActivity.Add (guild.Id, new Dictionary<ulong, DateTime> ());

            if (!userActivity [ guild.Id ].ContainsKey (user.Id))
                userActivity [ guild.Id ].Add (user.Id, DateTime.Now.AddYears (-1));
            else
                userActivity [ guild.Id ] [ user.Id ] = DateTime.Now;

            try {
                await UpdateUser (user);
            } catch (Exception exc) {
                Log.Write (exc);
            }

            if (single) {
                SaveData ();
            }
        }

        public DateTime GetLastActivity(SocketGuildUser user) {
            return userActivity[user.Guild.Id] [user.Id];
        }

        private async Task UpdateUser(SocketGuildUser user) {
            DateTime activity = GetLastActivity (user);
            DateTime now = DateTime.Now;

            ActivityRole [ ] activityStates = activityRoles.GetEntry (user.Guild);
            SocketRole [ ] roles = activityStates.Select (x => user.Guild.GetRole (x.id)).ToArray ();

            SocketRole finalRole = roles [ 0 ];

            DateTime lastDate = now.AddDays (1);

            for (int i = 0; i < activityStates.Length; i++) {
                DateTime thisDate = now.AddDays (-activityStates [ i ].threshold);

                if (activity < lastDate && activity > thisDate) {
                    finalRole = roles [ i ];
                    lastDate = thisDate;
                }
            }

            if (activity < now.AddDays (-activityStates.Last ().threshold))
                finalRole = roles.Last ();

            List<SocketRole> toRemove = roles.ToList ();
            toRemove.Remove (finalRole);

            await user.AsyncSecureAddRole (finalRole);
            toRemove.ForEach (x => user.AsyncSecureRemoveRole (x));
        }

        public async void Tick(DateTime lastTick, DateTime now) {
            if (lastTick.Day != now.Day) {

                await UpdateAll ();
                SaveData ();
            }
        }

        private async Task UpdateAll () {
            List<SocketGuildUser> users = new List<SocketGuildUser> ();
            ParentBotClient.discordClient.Guilds.ToList ().ForEach (x => users.AddRange (x.Users));

            foreach (SocketGuildUser u in users) {
                if (!userActivity.ContainsKey (u.Id)) {
                    RecordActivity (u, DateTime.Now.AddMonths (-6), false);
                }

                await UpdateUser (u);
            }
        }

        public class ActivityRole {
            public ulong id;
            public uint threshold;

            public ActivityRole (ulong _id, uint _threshold) {
                id = _id;
                threshold = _threshold;
            }
        }
    }       
}
