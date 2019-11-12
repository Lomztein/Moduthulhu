using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.Config
{
    public class PluginConfig
    {
        private List<ConfigFunctionInfo> _configInfos = new List<ConfigFunctionInfo>();

        public ConfigFunctionInfo[] GetConfigInfo(string name, string identifier) => _configInfos.Where (x => x.Matches (name, identifier)).ToArray ();
        public ConfigFunctionInfo[] GetConfigInfo(string identifier) => _configInfos.Where (x => x.Matches (identifier)).ToArray();
        public ConfigFunctionInfo[] GetConfigInfo() => _configInfos.ToArray();

        public void Add (string name, string description, string identifier, Delegate action, Func<string> message, params string[] paramNames)
        {
            Log.Write(Log.Type.PLUGIN, "Adding new configuration info for " + identifier + ".");
            _configInfos.Add(new ConfigFunctionInfo(name, description, identifier, action, message, paramNames));
        }

        /// <summary>
        /// It is recommeded that you use the alternative overload instead, since this contains zero checks that make sure the delegate matches the parameters. However if you are confident that you know what you're doing, go ahead and use this.
        /// </summary>
        /// <param name="targetPlugin"></param>
        /// <param name="identifier"></param>
        /// <param name="delegate"></param>
        /// <param name="parameters"></param>
        public void Add(string name, string description, string identifier, Delegate @delegate, Func<string> message, params ConfigFunctionParam[] parameters)
        {
            Log.Write(Log.Type.PLUGIN, "Adding new configuration info for " + identifier + ".");
            _configInfos.Add(new ConfigFunctionInfo(name, description, identifier, @delegate, message, parameters));
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
