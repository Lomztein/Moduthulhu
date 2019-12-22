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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Modules.Clock.ActivityMonitor
{
    [Descriptor ("Lomztein", "User Activity Monitor", "Positively sinister sounding plugin that applies roles to people based on last date of activity.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/User%20Activity/UserActivityMonitorPlugin.cs")]
    [GDPR (GDPRCompliance.Partial, "User IDs are automatically stored on any user activty, in order to keep track of last user activity.")]
    public class UserActivityMonitorPlugin : PluginBase {

        private CachedValue<Dictionary<ulong, DateTime>> _userActivity;
        private CachedValue<List<ActivityRole>> _activityRoles;

        public override void Initialize () {
            AssertPermission(Discord.GuildPermission.ManageRoles);

            GuildHandler.MessageReceived += DiscordClient_MessageReceived;
            GuildHandler.UserVoiceStateUpdated += DiscordClient_UserVoiceStateUpdated;
            GuildHandler.UserJoined += DiscordClient_UserJoined;
            GuildHandler.RoleDeleted += GuildHandler_RoleDeleted;

            AddConfigInfo("Add Activity Role", "Add new role.", new Action<SocketRole, uint>((x, y) => _activityRoles.MutateValue(z => AddRole(new ActivityRole(x.Id, y)))), () => "Added new activity role", "Role", "Treshold (days)");
            AddConfigInfo("Add Activity Role", "Add new role.", new Action<ulong, uint>((x, y) => _activityRoles.MutateValue(z => AddRole(new ActivityRole(GuildHandler.GetRole (x).Id, y)))), () => "Added new activity role", "Role", "Treshold (days)");
            AddConfigInfo("Add Activity Role", "Add new role.", new Action<string, uint>((x, y) => _activityRoles.MutateValue(z => AddRole(new ActivityRole(GuildHandler.GetRole (x).Id, y)))), () => "Added new activity role", "Role", "Treshold (days)");
            AddConfigInfo("Add Activity Role", "Display roles", () => "Current activity roles:\n" + string.Join("\n", _activityRoles.GetValue().Select(x => x.ToString(GuildHandler)).ToArray()));

            AddConfigInfo("Remove Activity Role", "Remove role.", new Action<SocketRole>(x => _activityRoles.MutateValue(y => y.Remove(AssertActivityRoleExists (z => z.Id == x.Id)))), () => "Removed activity role", "Role");
            AddConfigInfo("Remove Activity Role", "Remove role.", new Action<string>(x => _activityRoles.MutateValue(y => y.Remove(AssertActivityRoleExists(z => z.Id == GuildHandler.GetRole (x).Id)))), () => "Removed activity role", "Role");
            AddConfigInfo("Remove Activity Role", "Remove role.", new Action<uint>(x => _activityRoles.MutateValue(y => y.Remove(AssertActivityRoleExists(z => z.Id == GuildHandler.GetRole (x).Id)))), () => "Removed activity role", "Role");

            _userActivity = GetDataCache("UserActivity", x => new Dictionary<ulong, DateTime>());
            _activityRoles = GetConfigCache("ActivityRoles", x => new List<ActivityRole>());

            GuildHandler.Clock.OnDayPassed += CheckAll;
            PerformFilterMissing();
        }

        private Task GuildHandler_RoleDeleted(SocketRole arg)
        {
            PerformFilterMissing();
            return Task.CompletedTask;
        }

        public override void Shutdown() {
            _ = UpdateAll();
            StoreData();

            GuildHandler.MessageReceived -= DiscordClient_MessageReceived;
            GuildHandler.UserVoiceStateUpdated -= DiscordClient_UserVoiceStateUpdated;
            GuildHandler.UserJoined -= DiscordClient_UserJoined;
            GuildHandler.RoleDeleted -= GuildHandler_RoleDeleted;

            GuildHandler.Clock.OnDayPassed -= CheckAll;
        }

        private void AddRole (ActivityRole newRole)
        {
            _activityRoles.MutateValue (x => x.Add (newRole));
        }

        private List<ActivityRole> FilterMissing(List<ActivityRole> current) => current.Where(x => GuildHandler.FindRole(x.Id) != null).ToList();
        private void PerformFilterMissing ()
        {
            var current = _activityRoles.GetValue();
            var filtered = FilterMissing(current);

            if (current.Count != filtered.Count)
            {
                _activityRoles.SetValue(filtered);
            }
        }

        private ActivityRole AssertActivityRoleExists (Predicate<ActivityRole> predicate)
        {
            var role = _activityRoles.GetValue().FirstOrDefault(x => predicate (x));
            if (role == null)
            {
                throw new InvalidOperationException("No such activity role is currently registered.");
            }
            else
            {
                return role;
            }
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
            {
                _userActivity.GetValue().Add(user.Id, time);
            }
            else
            {
                _userActivity.GetValue()[user.Id] = time;
            }

            await UpdateUser (user);
        }

        public DateTime GetLastActivity(SocketGuildUser user) {
            return _userActivity.GetValue()[user.Id];
        }

        private async Task UpdateUser(SocketGuildUser user) {

            DateTime activity = GetLastActivity (user);
            DateTime now = DateTime.Now;

            List<ActivityRole> activityStates = _activityRoles.GetValue ();
            activityStates.Sort(Comparer<ActivityRole>.Create((x, y) => (int)x.Treshold - (int)y.Treshold));
            SocketRole[] roles = activityStates.Select (x => user.Guild.GetRole (x.Id)).ToArray ();

            SocketRole finalRole = roles[0];

            DateTime lastDate = now.AddDays (1);

            for (int i = 0; i < activityStates.Count; i++) {
                DateTime thisDate = now.AddDays (-activityStates[i].Treshold);

                if (activity < lastDate && activity > thisDate) {
                    finalRole = roles[i];
                    lastDate = thisDate;
                }
            }

            if (activity < now.AddDays (-activityStates.Last ().Treshold))
            {
                finalRole = roles.Last();
            }

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

        public override JToken RequestUserData(ulong id)
        {
            if (_userActivity.GetValue ().ContainsKey (id))
            {
                return new JObject
                {
                    { "LastActivity", _userActivity.GetValue ()[id] }
                };
            }
            return null;
        }

        public override void DeleteUserData (ulong id)
        {
            if (_userActivity.GetValue ().ContainsKey (id))
            {
                _userActivity.MutateValue(x => x.Remove(id));
            }
        }

        private static DateTime GetDefaultDate () => DateTime.Now.AddYears (-1);

        private void StoreData() => _userActivity.Store();

        public class ActivityRole {
            [JsonProperty ("Id")]
            public ulong Id { get; private set; }
            [JsonProperty ("Treshold")]
            public uint Treshold { get; private set; }

            public ActivityRole (ulong _id, uint _threshold) {
                Id = _id;
                Treshold = _threshold;
            }

            public string ToString (GuildHandler roleSource)
            {
                SocketRole role = roleSource.GetRole(Id);
                return $"Role: {role.Name}, treshold: {Treshold} days.";
            }
        }
    }       
}
