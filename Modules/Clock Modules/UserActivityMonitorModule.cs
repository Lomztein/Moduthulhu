using Lomztein.Moduthulhu.Core.Module.Framework;
using Discord.WebSocket;
using Discord;
using System;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Bot;
using Discord.Rest;
using Lomztein.Moduthulhu.Cross;

namespace Lomztein.Moduthulhu.Modules.Clock.ActivityMonitor
{
    public class UserActivityMonitorModule : ModuleBase, ITickable, IConfigurable<MultiConfig> {

        public override string Name => "User Activity Module";
        public override string Description => "Catagorises people into configurable roles based on their last date of activity. Not recommended, but possible for multiserver.";
        public override string Author => "Lomztein";

        public override string [ ] RequiredModules => new string[] { "Lomztein_Clock Module"};

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private Dictionary<ulong, Dictionary<ulong, DateTime>> userActivity;
        [AutoConfig] private MultiEntry<ActivityRole[], SocketGuild> activityRoles = new MultiEntry<ActivityRole[], SocketGuild> (x => new ActivityRole[] { new ActivityRole (0, 7), new ActivityRole (0, 30) }, "ActivityRoles", true);

        private void SaveData () => DataSerialization.SerializeData (userActivity, "UserActivity");
        private void LoadData() {
            userActivity = DataSerialization.DeserializeData<Dictionary<ulong, Dictionary<ulong, DateTime>>> ("UserActivity");
            if (userActivity == null)
                userActivity = new Dictionary<ulong, Dictionary<ulong, DateTime>> ();
        }

        public override void Initialize () {

            LoadData ();

            ParentBotClient.discordClient.MessageReceived += DiscordClient_MessageReceived;
            ParentBotClient.discordClient.UserVoiceStateUpdated += DiscordClient_UserVoiceStateUpdated;
            ParentBotClient.discordClient.UserJoined += DiscordClient_UserJoined;
        }

        public override void Shutdown() {
            ParentBotClient.discordClient.MessageReceived += DiscordClient_MessageReceived;
            ParentBotClient.discordClient.UserVoiceStateUpdated += DiscordClient_UserVoiceStateUpdated;
            ParentBotClient.discordClient.UserJoined += DiscordClient_UserJoined;
        }

        private Task DiscordClient_UserJoined(SocketGuildUser arg) {
            RecordActivity (arg, DateTime.Now);
            return Task.CompletedTask;
        }

        private Task DiscordClient_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3) {
            SocketGuildUser afterUser = arg1 as SocketGuildUser;
            if (afterUser?.VoiceChannel != null) {
                RecordActivity (afterUser, DateTime.Now);
            }

            return Task.CompletedTask;
        }

        private Task DiscordClient_MessageReceived(SocketMessage arg) {
            RecordActivity (arg.Author as SocketGuildUser, DateTime.Now);
            return Task.CompletedTask;
        }

        public override void PostInitialize () {
            ParentModuleHandler.GetModule<ClockModule> ().AddTickable (this);
        }

        public async void RecordActivity(SocketGuildUser user, DateTime time) {
            if (user == null)
                return;

            if (this.IsConfigured (user.Guild.Id))
                return;

            SocketGuild guild = user.Guild;

            if (!userActivity.ContainsKey (guild.Id))
                userActivity.Add (guild.Id, new Dictionary<ulong, DateTime> ());

            if (!userActivity [ guild.Id ].ContainsKey (user.Id))
                userActivity [ guild.Id ].Add (user.Id, time);
            else
                userActivity [ guild.Id ] [ user.Id ] = time;

            try {
                await UpdateUser (user);
            } catch (Exception exc) {
                Log.Write (exc);
            }
        }

        public DateTime GetLastActivity(SocketGuildUser user) {
            return userActivity[user.Guild.Id] [user.Id];
        }

        private async Task UpdateUser(SocketGuildUser user) {

            try {
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

            } catch (Exception e) {
                Log.Write (e);
            }

        }

        public async void Tick(DateTime lastTick, DateTime now) {
            if (ClockModule.DayPassed (lastTick, now)) {

                await UpdateAll ();
                SaveData ();
            }
        }

        private async Task UpdateAll () {
            List<SocketGuildUser> users = new List<SocketGuildUser> ();
            ParentBotClient.discordClient.Guilds.ToList ().ForEach (x => users.AddRange (x.Users));

            foreach (SocketGuildUser u in users) {

                if (!userActivity.ContainsKey (u.Guild.Id)) {
                    RecordActivity (u, DateTime.Now);
                }

                if (!userActivity[u.Guild.Id].ContainsKey (u.Id)) {
                    RecordActivity (u, DateTime.Now);
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
