using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;

namespace Lomztein.Moduthulhu.Modules.Clock.ActivityMonitor
{
    [Descriptor ("Lomztein", "User Activity Monitor", "Positively sinister sounding plugin that applies roles to people based on last date of activity.")]
    public class UserActivityMonitorPlugin : PluginBase {

        private CachedValue<Dictionary<ulong, DateTime>> _userActivity;
        private CachedValue<List<ActivityRole>> _activityRoles;

        public override void Initialize () {
            GuildHandler.MessageReceived += DiscordClient_MessageReceived;
            GuildHandler.UserVoiceStateUpdated += DiscordClient_UserVoiceStateUpdated;
            GuildHandler.UserJoined += DiscordClient_UserJoined;

            AddConfigInfo("Add Activity Role", "Add new role.", new Action<SocketRole, uint>((x, y) => _activityRoles.MutateValue(z => AddRole(new ActivityRole((x?.Id).GetValueOrDefault (), y)))), () => "Added new activity role", "Role", "Treshold (days)");
            AddConfigInfo("Add Activity Role", "Add new role.", new Action<ulong, uint>((x, y) => _activityRoles.MutateValue(z => AddRole(new ActivityRole(x, y)))), () => "Added new activity role", "Role", "Treshold (days)");
            AddConfigInfo("Add Activity Role", "Add new role.", new Action<string, uint>((x, y) => _activityRoles.MutateValue(z => AddRole(new ActivityRole((GuildHandler.FindRole (x)?.Id).GetValueOrDefault (), y)))), () => "Added new activity role", "Role", "Treshold (days)");
            AddConfigInfo("Add Activity Role", "Display roles", () => "Current activity roles:\n" + string.Join("\n", _activityRoles.GetValue().Select(x => x.ToString(GuildHandler)).ToArray()));

            AddConfigInfo("Remove Activity Role", "Remove role.", new Action<SocketRole>((x) => _activityRoles.MutateValue(z => z.RemoveAll(w => w.id == x.Id))), () => "Removed activity role", "Role");
            AddConfigInfo("Remove Activity Role", "Remove role.", new Action<string>((x) => _activityRoles.MutateValue(z => z.RemoveAll(w => w.id == GuildHandler.FindRole (x).Id))), () => "Removed activity role", "Role");
            AddConfigInfo("Remove Activity Role", "Remove role.", new Action<uint>((x) => _activityRoles.MutateValue(z => z.RemoveAll(w => w.id == x))), () => "Removed activity role", "Role");

            _userActivity = GetDataCache("UserActivity", x => new Dictionary<ulong, DateTime>());
            _activityRoles = GetConfigCache("ActivityRoles", x => new List<ActivityRole>());

            GuildHandler.Clock.OnDayPassed += CheckAll;
        }

        public override void Shutdown() {
            GuildHandler.MessageReceived -= DiscordClient_MessageReceived;
            GuildHandler.UserVoiceStateUpdated -= DiscordClient_UserVoiceStateUpdated;
            GuildHandler.UserJoined -= DiscordClient_UserJoined;

            GuildHandler.Clock.OnDayPassed -= CheckAll;
        }

        private void AddRole (ActivityRole newRole)
        {
            if (GuildHandler.GetRole (newRole.id) == null)
            {
                throw new InvalidOperationException("No such role exists.");
            }
            _activityRoles.MutateValue (x => x.Add (newRole));
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
            
            if (!_userActivity.GetValue ().ContainsKey (user.Id))
                _userActivity.GetValue().Add (user.Id, time);
            else
                _userActivity.GetValue()[user.Id] = time;

            await UpdateUser (user);
        }

        public DateTime GetLastActivity(SocketGuildUser user) {
            return _userActivity.GetValue()[user.Id];
        }

        private async Task UpdateUser(SocketGuildUser user) {

            DateTime activity = GetLastActivity (user);
            DateTime now = DateTime.Now;

            List<ActivityRole> activityStates = _activityRoles.GetValue ();
            activityStates.Sort(Comparer<ActivityRole>.Create((x, y) => (int)x.threshold - (int)y.threshold));
            SocketRole[] roles = activityStates.Select (x => user.Guild.GetRole (x.id)).ToArray ();

            SocketRole finalRole = roles[0];

            DateTime lastDate = now.AddDays (1);

            for (int i = 0; i < activityStates.Count; i++) {
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

        public async Task CheckAll(DateTime lastTick, DateTime now) {
            await UpdateAll ();
            StoreData ();
        }

        private async Task UpdateAll () {
            List<SocketGuildUser> users = GuildHandler.GetGuild().Users.ToList();

            foreach (SocketGuildUser u in users) {
                if (!_userActivity.GetValue().ContainsKey (u.Id)) {
                    await RecordActivity (u, GetDefaultDate ());
                }
            }
        }

        private DateTime GetDefaultDate () => DateTime.Now.AddYears (-1);

        private void StoreData() => _userActivity.Store();

        public class ActivityRole {
            public ulong id;
            public uint threshold;

            public ActivityRole (ulong _id, uint _threshold) {
                id = _id;
                threshold = _threshold;
            }

            public string ToString (GuildHandler roleSource)
            {
                SocketRole role = roleSource.GetRole(id);
                return $"Role: {role.Name}, treshold: {threshold} days.";
            }
        }
    }       
}
