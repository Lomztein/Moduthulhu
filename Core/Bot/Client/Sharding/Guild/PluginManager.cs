using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginManager
    {
        private readonly List<IPlugin> _activePlugins;
        public IPlugin[] ActivePlugins => _activePlugins.ToArray();

        private GuildHandler _parentHandler;

        private CachedValue<string[]> _enabledPlugins;

        public PluginManager (GuildHandler parent)
        {
            _parentHandler = parent;
            _enabledPlugins = new CachedValue<string[]>(new IdentityKeyJsonRepository("plugindata"), _parentHandler.GuildId, "EnabledPlugins", () => Array.Empty<string>());
        }
    }
}
