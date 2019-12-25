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
using Lomztein.Moduthulhu.Core.Extensions;

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

            AddConfigInfo("List Activity Roles", "Display roles", () => "Current activity roles:\n" + string.Join("\n", _activityRoles.GetValue().Select(x => x.ToString(GuildHandler)).ToArray()));

            AddConfigInfo<SocketRole, uint>("Add Activity Role", "Add new role.", (x, y) => AddRole(new ActivityRole(x.Id, y)), (x, y) => $"Added new activity role '{x.Name}' with a treshold of {y} days.", "Role", "Treshold (days)");
            AddConfigInfo<ulong, uint>("Add Activity Role", "Add new role.", (x, y) => AddRole(new ActivityRole(GuildHandler.GetRole (x).Id, y)), (x, y) => $"Added new activity role '{GuildHandler.GetRole(x).Name}' with a treshold of {y} days.", "Role", "Treshold (days)");
            AddConfigInfo<string, uint>("Add Activity Role", "Add new role.", (x, y) => AddRole(new ActivityRole(GuildHandler.GetRole (x).Id, y)), (x, y) => $"Added new activity role '{GuildHandler.GetRole(x).Name}' with a treshold of {y} days.", "Role", "Treshold (days)");

            AddConfigInfo<SocketRole>("Remove Activity Role", "Remove role.", x => RemoveRole(x.Id), x => $"Removed activity role {x.Name}.", "Role");
            AddConfigInfo<string>("Remove Activity Role", "Remove role.", x => RemoveRole(GuildHandler.GetRole(x).Id), x => $"Removed activity role '{x}'", "Role");

            _userActivity = GetDataCache("UserActivity", x => new Dictionary<ulong, DateTime>());
            _activityRoles = GetConfigCache("ActivityRoles", x => new List<ActivityRole>());

            GuildHandler.Clock.OnDayPassed += CheckAll;
            PerformFilterMissing();
        }

        public override void PostInitialize()
        {
            AddGeneralFeaturesStateAttribute("UserActivityMonitor", "Automated assigning roles to users based on last date of activity.");
            _ = UpdateAll();
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
            await RecordActivity (arg.Id, DateTime.Now);
        }

        private async Task DiscordClient_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3) {
            await RecordActivity (arg1.Id, DateTime.Now);
        }

        private async Task DiscordClient_MessageReceived(SocketMessage arg) {
            await RecordActivity (arg.Author.Id, DateTime.Now);
        }

        public async Task RecordActivity(ulong user, DateTime time) {
            
            if (!_userActivity.GetValue ().ContainsKey (user))
            {
                _userActivity.GetValue().Add(user, time);
            }
            else
            {
                _userActivity.GetValue()[user] = time;
            }

            await UpdateUser (user);
        }

        public DateTime GetLastActivity(ulong user) {
            return _userActivity.GetValue()[user];
        }

        private async Task UpdateUser(ulong user) {

            DateTime activity = GetLastActivity(user);
            ulong role = GetRole(_activityRoles.GetValue(), activity);
            SocketRole roleObj = GuildHandler.FindRole(role);
            Core.Log.Plugin($"User with Id {user} activityrole has been evaluated to {roleObj?.Name.ToStringOrNull ()}.");

            if (roleObj != null)
            {
                SocketGuildUser userObj = GuildHandler.GetUser(user);
                List<SocketRole> toRemove = userObj.Roles.Where(x => _activityRoles.GetValue().Any(y => x.Id == y.Id)).ToList ();
                toRemove.Remove(roleObj);

                if (!userObj.Roles.Contains (roleObj))
                {
                    Core.Log.User($"Adding activity role {roleObj.Name} to {userObj.GetShownName ()}");
                    await userObj.AsyncSecureAddRole(roleObj);
                }
                foreach (var toRemoveRole in toRemove)
                {
                    Core.Log.User($"Removing activity role {toRemoveRole.Name} from {userObj.GetShownName()}");
                    await userObj.AsyncSecureRemoveRole(toRemoveRole);
                }
            }
        }

        private void AddRole (ActivityRole role)
        {
            _activityRoles.MutateValue(x => x.Add(role));
            SortRoles();
        } 

        private void RemoveRole (ulong role)
        {
            if (_activityRoles.GetValue().Any (x => x.Id == role))
            {
                _activityRoles.MutateValue(x => x.RemoveAll(y => y.Id == role));
                SortRoles();
            }
            else
            {
                throw new InvalidOperationException("Cannot remove role, role currently isn't in the list of Activity Roles.");
            }
        }

        private void SortRoles ()
        {
            _activityRoles.GetValue ().Sort(Comparer<ActivityRole>.Create((x, y) => (int)x.Treshold - (int)y.Treshold));
        }

        public static ulong GetRole (List<ActivityRole> roles, DateTime activity)
        {
            ulong[] roleObjs = roles.Select(x => x.Id).ToArray();
            DateTime now = DateTime.Now;

            ulong finalRole = roleObjs[0];
            DateTime lastDate = now.AddDays(1);

            for (int i = 0; i < roles.Count; i++)
            {
                DateTime thisDate = now.AddDays(-roles[i].Treshold);

                if (activity < lastDate && activity > thisDate)
                {
                    finalRole = roleObjs[i];
                    lastDate = thisDate;
                }
            }

            if (activity < now.AddDays(-roles.Last().Treshold))
            {
                finalRole = roleObjs.Last();
            }
            return finalRole;
        }

        public async Task CheckAll(DateTime lastTick, DateTime now) {
            Core.Log.Plugin("Checking all users for their last activity.");
            await UpdateAll ();
            StoreData ();
        }

        private async Task UpdateAll () {
            await GuildHandler.GetGuild().DownloadUsersAsync();
            List<SocketGuildUser> users = GuildHandler.GetGuild().Users.ToList();

            foreach (SocketGuildUser u in users) {
                Core.Log.Plugin($"Checking user {u.GetShownName ()}.");
                if (!_userActivity.GetValue().ContainsKey (u.Id))
                {
                    Core.Log.Plugin($"No last date for user {u.GetShownName ()}. Recording default date.");
                    await RecordActivity (u.Id, GetDefaultDate ());
                }
                else
                {
                    Core.Log.Plugin($"No last date for user {u.GetShownName ()}. Recording default date.");
                    await UpdateUser(u.Id);
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
