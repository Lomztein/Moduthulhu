using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.Config
{
    public class PluginConfig
    {
        private readonly List<ConfigFunctionInfo> _configInfos = new List<ConfigFunctionInfo>();

        public ConfigFunctionInfo[] GetConfigInfo(string name, string identifier) => _configInfos.Where (x => x.Matches (name, identifier)).ToArray ();
        public ConfigFunctionInfo[] GetConfigInfo(string identifier) => _configInfos.Where (x => x.Matches (identifier)).ToArray();
        public ConfigFunctionInfo[] GetConfigInfo() => _configInfos.ToArray();

        public void Add (string name, string description, string identifier, Delegate action, Delegate message, params string[] paramNames)
        {
            Log.Write(Log.Type.PLUGIN, "Adding new configuration info for " + identifier + ".");
            _configInfos.Add(new ConfigFunctionInfo(name, description, identifier, action, message, paramNames));
        }

        public void Remove (string name, string identifier)
        {
            Log.Write(Log.Type.PLUGIN, $"Removing configuration info 'name' for {identifier}.");
            var info = _configInfos.Find(x => x.Matches(name, identifier));
            _configInfos.Remove(info);
        }

        public void Clear (string identifier)
        {
            Log.Write(Log.Type.PLUGIN, "Clearing configuration info for " + identifier + ".");
            _configInfos.RemoveAll(x => x.Matches (identifier));
        }
    }
}
