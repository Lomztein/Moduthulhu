using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
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

        protected void Log(string contents) => Plugin.Log (this, contents);

        protected CachedValue<T> GetDataCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new IdentityKeyJsonRepository("plugindata"), GuildHandler.GuildId, key, () => defaultValue(GuildHandler));
        protected CachedValue<T> GetConfigCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new IdentityKeyJsonRepository("pluginconfig"), GuildHandler.GuildId, key, () => defaultValue(GuildHandler));

        protected void RegisterMessageFunction(string identifier, Func<object, object> function) => GuildHandler.Plugins.RegisterMessageFunction(Regex.Replace (Name, " ", "") + "." + identifier, function);
        protected void RegisterMessageAction(string identifier, Action<object> action) => GuildHandler.Plugins.RegisterMessageAction(Regex.Replace(Name, " ", "") + "." + identifier, action);

        protected void UnregisterMessageFunction(string identifier) => GuildHandler.Plugins.UnregisterMessageFunction(identifier);
        protected void UnregisterMessageAction(string identifier) => GuildHandler.Plugins.UnregisterMessageAction(identifier);

        protected void SendMessage(string identifier, object value = null) => GuildHandler.Plugins.SendMessage(identifier, value);
        protected T SendMessage<T>(string identifier, object value = null) => GuildHandler.Plugins.SendMessage<T>(identifier, value);
    }
}
