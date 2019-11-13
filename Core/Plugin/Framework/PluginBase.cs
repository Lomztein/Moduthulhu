using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public abstract class PluginBase : IPlugin {

        [Obsolete ("Use Plugin.GetName instead.")]
        public string Name => Plugin.GetName(GetType ());
        [Obsolete ("Use Plugin.GetDescription instead.")]
        public string Description => Plugin.GetDescription(GetType());
        [Obsolete ("Use Plugin.GetAuthor instead.")]
        public string Author => Plugin.GetAuthor(GetType());
        [Obsolete ("Use Plugin.GetVersion instead.")]
        public string Version => Plugin.GetVersion(GetType());

        [Obsolete ("Use Plugin.GetAuthorURI instead.")]
        public Uri AuthorURI => Plugin.GetAuthorURI(GetType());
        [Obsolete ("Use Plugin.GetPatchURI instead.")]
        public Uri PatchURI => Plugin.GetPatchURI(GetType());
        [Obsolete ("Use Plugin.GetProjectURI instead.")]
        public Uri ProjectURI => Plugin.GetProjectURI(GetType());

        public GuildHandler GuildHandler { get; private set; }
        public abstract void Initialize ();

        public virtual void PreInitialize (GuildHandler handler) => GuildHandler = handler;
        public virtual void PostInitialize () { }

        public abstract void Shutdown();

        protected void Log(string contents) => Plugin.Log (this, contents);

        protected CachedValue<T> GetDataCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new IdentityKeyJsonRepository("plugindata"), GuildHandler.GuildId, Plugin.GetFullName (GetType ()) + "." + key, () => defaultValue(GuildHandler));
        protected CachedValue<T> GetConfigCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new IdentityKeyJsonRepository("pluginconfig"), GuildHandler.GuildId, Plugin.GetFullName(GetType()) + "." + key, () => defaultValue(GuildHandler));

        protected void RegisterMessageFunction(string identifier, Func<object, object> function) => GuildHandler.Messenger.Register(Plugin.GetFullName (GetType ()), identifier, function);
        protected void RegisterMessageAction(string identifier, Action<object> action) => GuildHandler.Messenger.Register(Plugin.GetFullName(GetType()), identifier, action);

        protected void UnregisterMessageDelegate(string identifier) => GuildHandler.Messenger.Unregister(Plugin.GetFullName(GetType()), identifier);
        protected void ClearMessageDelegates() => GuildHandler.Messenger.Clear(Plugin.GetFullName(GetType ()));

        protected void AddConfigInfo(string name, string description, Delegate @delegate, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), @delegate, () => "Succesfully altered configuration.", paramNames);
        protected void AddConfigInfo(string name, string description, Delegate @delegate, Func<string> message, params string[] paramNames) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), @delegate, message, paramNames);
        protected void AddConfigInfo(string name, string description, Func<string> message) => GuildHandler.Config.Add(name, description, Plugin.GetFullName(GetType()), new Action(() => { }), message, Array.Empty<string> ());
        protected void ClearConfigInfos() => GuildHandler.Config.Clear(Plugin.GetFullName(GetType()));

        protected void SendMessage(string target, string identifier, object value = null) => GuildHandler.Messenger.SendMessage(target, identifier, value);
        protected T SendMessage<T>(string target, string identifier, object value = null) => GuildHandler.Messenger.SendMessage<T>(target, identifier, value);
    }
}
