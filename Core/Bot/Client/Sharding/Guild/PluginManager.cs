using Lomztein.Moduthulhu.Core.Plugin;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginManager
    {
        private List<IPlugin> _activePlugins;
        public IPlugin[] ActivePlugins => _activePlugins.ToArray();



    }
}
