using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public abstract class PluginBase : IPlugin {

        public string Name => Plugin.GetName(GetType());
        public string Description => Plugin.GetDescription(GetType());
        public string Author => Plugin.GetAuthor(GetType());
        public string Version => Plugin.GetVersion(GetType());

        public Uri AuthorURI => Plugin.GetAuthorURI(GetType());
        public Uri PatchURI => Plugin.GetPatchURI(GetType());
        public Uri ProjectURI => Plugin.GetProjectURI(GetType());

        public GuildHandler GuildHandler { get; private set; }
        public abstract void Initialize();

        public virtual void PreInitialize(GuildHandler handler) => GuildHandler = handler;
        public virtual void PostInitialize() { }

        public abstract void Shutdown();

        public virtual JToken RequestUserData(ulong id) => null;
        public virtual void DeleteUserData(ulong id) { }

        protected void Log(string contents) => Plugin.Log(this, contents);

        protected CachedValue<T> GetDataCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new DoubleKeyJsonRepository("plugindata"), GuildHandler.GuildId, Plugin.GetFullName(GetType()) + "." + key, () => defaultValue(GuildHandler));
        protected CachedValue<T> GetConfigCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new DoubleKeyJsonRepository("pluginconfig"), GuildHandler.GuildId, Plugin.GetFullName(GetType()) + "." + key, () => defaultValue(GuildHandler));

        protected void RegisterMessageFunction(string identifier, Func<object, object> function) => GuildHandler.Messenger.Register(Plugin.GetFullName(GetType()), identifier, function);
        protected void RegisterMessageAction(string identifier, Action<object> action) => GuildHandler.Messenger.Register(Plugin.GetFullName(GetType()), identifier, action);

        protected void UnregisterMessageDelegate(string identifier) => GuildHandler.Messenger.Unregister(Plugin.GetFullName(GetType()), identifier);
        protected void ClearMessageDelegates() => GuildHandler.Messenger.Clear(Plugin.GetFullName(GetType()));

        protected void AddConfigInfo(string name, string description, Action action, Func<string> message) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message);
        protected void AddConfigInfo(string name, string description, Delegate action, Delegate message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo(string name, string description, Func<string> message) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), new Action(() => { }), message);

        // Generic AddConfigInfo methods.
        #region
        protected void AddConfigInfo<T1>(string name, string description, Action<T1> action, Func<T1, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2>(string name, string description, Action<T1, T2> action, Func<T1, T2, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3>(string name, string description, Action<T1, T2, T3> action, Func<T1, T2, T3, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4>(string name, string description, Action<T1, T2, T3, T4> action, Func<T1, T2, T3, T4, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5>(string name, string description, Action<T1, T2, T3, T4, T5> action, Func<T1, T2, T3, T4, T5, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5, T6>(string name, string description, Action<T1, T2, T3, T4, T5, T6> action, Func<T1, T2, T3, T4, T5, T6, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5, T6, T7>(string name, string description, Action<T1, T2, T3, T4, T5, T6, T7> action, Func<T1, T2, T3, T4, T5, T6, T7, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        protected void AddConfigInfo<T1, T2, T3, T4, T5, T6, T7, T8>(string name, string description, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), action, message, paramNames);
        #endregion

        protected void ClearConfigInfos() => GuildHandler.Config.Clear(Plugin.GetFullName(GetType()));

        protected void SendMessage(string target, string identifier) => GuildHandler.Messenger.SendMessage(target, identifier);
        protected void SendMessage(string target, string identifier, object value) => GuildHandler.Messenger.SendMessage(target, identifier, value);
        protected T SendMessage<T>(string target, string identifier) => GuildHandler.Messenger.SendMessage<T>(target, identifier);
        protected T SendMessage<T>(string target, string identifier, object value) => GuildHandler.Messenger.SendMessage<T>(target, identifier, value);

        protected void AssertPermission(GuildPermission perm) => GuildHandler.AssertPermission(perm);
        protected void AssertChannelPermission(ChannelPermission perm, ulong channelId) => GuildHandler.AssertChannelPermission(perm, channelId);
        protected bool HasPermission(GuildPermission perm) => GuildHandler.HasPermission(perm);
        protected void HasChannelPermission(ChannelPermission perm, ulong channelId) => GuildHandler.HasChannelPermission(perm, channelId);

        protected void AddStateAttribute(string identifier, string name, string description) => GuildHandler.AddStateAttribute(Plugin.GetVersionedFullName(GetType())+identifier, name, description);
        protected void AddGeneralFeaturesStateAttribute(string name, string description) => GuildHandler.AddStateAttribute("GeneralFeatures", name, description);
        protected void SetStateChangeHeaders(string identifier, string addition, string removals, string mutations) => GuildHandler.SetStateChangeHeaders(Plugin.GetVersionedFullName(GetType())+identifier, addition, removals, mutations);
        protected void SetStateChangeHeaders(string identifier, string addition, string removals) => GuildHandler.SetStateChangeHeaders(Plugin.GetVersionedFullName(GetType())+identifier, addition, removals, string.Empty);

        protected Task NotifyGuild(string message, Embed embed) => GuildHandler.Notifier.Notify(message, embed);
        protected Task NotifyGuild(string message) => GuildHandler.Notifier.Notify(message);
        protected Task NotifyGuild(Embed embed) => GuildHandler.Notifier.Notify(embed);

        protected void DisablePlugin(string message)
        {
            GuildHandler.Plugins.RemovePlugin(Plugin.GetName(GetType()));
            GuildHandler.Plugins.ReloadPlugins();
            throw new PluginDisabledException(message);
        }

        protected void DisablePluginIfPermissionMissing(GuildPermission permission, bool notifyGuild)
        {
            if (!HasPermission(permission))
            {
                if (notifyGuild)
                {
                    NotifyGuild($"Plugin {Plugin.GetName(GetType())} requires permission '{permission}' to function, which has been revoked. The plugin has automatically been disabled.");
                }
                DisablePlugin($"Disabled plugin due to revoked permission '{permission}'.");
            }
        }
    }
}
