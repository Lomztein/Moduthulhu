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
    public class UserActivityMonitorModule : ModuleBase, IConfigurable<MultiConfig> {

        public override string Name => "User Activity Module";
        public override string Description => "Categorises people into configurable roles based on their last date of activity.";
        public override string Author => "Lomztein";

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
            ParentShard.MessageReceived += DiscordClient_MessageReceived;
            ParentShard.UserVoiceStateUpdated += DiscordClient_UserVoiceStateUpdated;
            ParentShard.UserJoined += DiscordClient_UserJoined;
            this.GetClock ().OnDayPassed += CheckAll;
        }

        public override void Shutdown() {
            ParentShard.MessageReceived -= DiscordClient_MessageReceived;
            ParentShard.UserVoiceStateUpdated -= DiscordClient_UserVoiceStateUpdated;
            ParentShard.UserJoined -= DiscordClient_UserJoined;
            this.GetClock ().OnDayPassed -= CheckAll;
        }

        private async Task DiscordClient_UserJoined(SocketGuildUser arg) {
            await RecordActivity (arg, DateTime.Now);
        }

        private async Task DiscordClient_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3) {
            SocketGuildUser afterUser = arg1 as SocketGuildUser;
            if (afterUser?.VoiceChannel != null) {
                await RecordActivity (afterUser, DateTime.Now);
            }
        }

        private async Task DiscordClient_MessageReceived(SocketMessage arg) {
            await RecordActivity (arg.Author as SocketGuildUser, DateTime.Now);
        }

        public async Task RecordActivity(SocketGuildUser user, DateTime time) {
            if (user == null)
                return;

            if (this.IsConfigured (user.Guild.Id))
                return;

            SocketGuild guild = user.Guild;

            if (!userActivity.ContainsKey (guild.Id))
                userActivity.Add (guild.Id, new Dictionary<ulong, DateTime> ());

            if (!userActivity[guild.Id].ContainsKey (user.Id))
                userActivity[guild.Id].Add (user.Id, time);
            else
                userActivity[guild.Id][user.Id] = time;

            await UpdateUser (user);
        }

        public DateTime GetLastActivity(SocketGuildUser user) {
            return userActivity[user.Guild.Id] [user.Id];
        }

        private async Task UpdateUser(SocketGuildUser user) {

            DateTime activity = GetLastActivity (user);
            DateTime now = DateTime.Now;

            ActivityRole[] activityStates = activityRoles.GetEntry (user.Guild);
            SocketRole[] roles = activityStates.Select (x => user.Guild.GetRole (x.id)).ToArray ();

            SocketRole finalRole = roles[0];

            DateTime lastDate = now.AddDays (1);

            for (int i = 0; i < activityStates.Length; i++) {
                DateTime thisDate = now.AddDays (-activityStates[i].threshold);

                if (activity < lastDate && activity > thisDate) {
                    finalRole = roles[i];
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

        public async void CheckAll(DateTime lastTick, DateTime now) {
            await UpdateAll ();
            SaveData ();
        }

        private async Task UpdateAll () {
            List<SocketGuildUser> users = new List<SocketGuildUser> ();
            ParentShard.Guilds.ToList ().ForEach (x => users.AddRange (x.Users));

            foreach (SocketGuildUser u in users) {

                if (!userActivity.ContainsKey (u.Guild.Id)) {
                    await RecordActivity (u, DateTime.Now);
                }

                if (!userActivity[u.Guild.Id].ContainsKey (u.Id)) {
                    await RecordActivity (u, DateTime.Now);
                }
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
