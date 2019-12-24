using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

        public string Name => Plugin.GetName(GetType ());
        public string Description => Plugin.GetDescription(GetType());
        public string Author => Plugin.GetAuthor(GetType());
        public string Version => Plugin.GetVersion(GetType());

        public Uri AuthorURI => Plugin.GetAuthorURI(GetType());
        public Uri PatchURI => Plugin.GetPatchURI(GetType());
        public Uri ProjectURI => Plugin.GetProjectURI(GetType());

        public GuildHandler GuildHandler { get; private set; }
        public abstract void Initialize ();

        public virtual void PreInitialize (GuildHandler handler) => GuildHandler = handler;
        public virtual void PostInitialize () { }

        public abstract void Shutdown();

        public virtual JToken RequestUserData(ulong id) => null;
        public virtual void DeleteUserData(ulong id) { }

        protected void Log(string contents) => Plugin.Log (this, contents);

        protected CachedValue<T> GetDataCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new DoubleKeyJsonRepository("plugindata"), GuildHandler.GuildId, Plugin.GetFullName (GetType ()) + "." + key, () => defaultValue(GuildHandler));
        protected CachedValue<T> GetConfigCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new DoubleKeyJsonRepository("pluginconfig"), GuildHandler.GuildId, Plugin.GetFullName(GetType()) + "." + key, () => defaultValue(GuildHandler));

        protected void RegisterMessageFunction(string identifier, Func<object, object> function) => GuildHandler.Messenger.Register(Plugin.GetFullName (GetType ()), identifier, function);
        protected void RegisterMessageAction(string identifier, Action<object> action) => GuildHandler.Messenger.Register(Plugin.GetFullName(GetType()), identifier, action);

        protected void UnregisterMessageDelegate(string identifier) => GuildHandler.Messenger.Unregister(Plugin.GetFullName(GetType()), identifier);
        protected void ClearMessageDelegates() => GuildHandler.Messenger.Clear(Plugin.GetFullName(GetType ()));

        protected void AddConfigInfo(string name, string description, Delegate @delegate, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), @delegate, () => "Succesfully altered configuration.", paramNames);
        protected void AddConfigInfo(string name, string description, Delegate @delegate, Func<string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), @delegate, message, paramNames);
        protected void AddConfigInfo(string name, string description, Func<string> message) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), new Action(() => { }), message, Array.Empty<string> ());
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
    }
}
