using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    public abstract class PluginBase : IPlugin {

        public string Name => Plugin.GetName(this);
        public string Description => Plugin.GetDescription(this);
        public string Author => Plugin.GetAuthor(this);
        public string Version => Plugin.GetVersion(this);

        public Uri AuthorURI => Plugin.GetAuthorURI(this);
        public Uri PatchURI => Plugin.GetPatchURI(this);
        public Uri ProjectURI => Plugin.GetProjectURI(this);

        public GuildHandler GuildHandler { get; private set; }

        public abstract void Initialize ();

        public virtual void PreInitialize (GuildHandler handler) => GuildHandler = handler;
        public virtual void PostInitialize () { }

        public abstract void Shutdown();

        protected void Log(string contents) => Plugin.Log (this, contents);

        protected CachedValue<T> GetDataCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new IdentityKeyJsonRepository("plugindata"), GuildHandler.GuildId, key, () => defaultValue(GuildHandler));
        protected CachedValue<T> GetConfigCache<T>(string key, Func<GuildHandler, T> defaultValue) => new CachedValue<T>(new IdentityKeyJsonRepository("pluginconfig"), GuildHandler.GuildId, key, () => defaultValue(GuildHandler));
    }
}
