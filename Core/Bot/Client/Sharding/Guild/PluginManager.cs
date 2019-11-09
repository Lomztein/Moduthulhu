using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginManager
    {
        private readonly List<IPlugin> _activePlugins = new List<IPlugin>();

        private GuildHandler _parentHandler;

        private CachedValue<List<string>> _enabledPlugins;

        public PluginManager (GuildHandler parent)
        {
            _parentHandler = parent;
            _enabledPlugins = new CachedValue<List<string>>(new IdentityKeyJsonRepository("plugindata"), _parentHandler.GuildId, "EnabledPlugins", () => PluginLoader.GetPlugins().Where (x => PluginLoader.IsStandard (x)).Select (y => Plugin.GetVersionedFullName (y)).ToList ());
        }

        public IPlugin[] GetActivePlugins() => _activePlugins.ToArray();

        public void ShutdownPlugins ()
        {
            Log.Write(Log.Type.PLUGIN, "Shutting down plugins for handler " + _parentHandler.Name);
            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Shutting down plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.Shutdown();
            }
            _activePlugins.Clear();
        }

        public void ReloadPlugins ()
        {
            ShutdownPlugins();
            Log.Write(Log.Type.PLUGIN, "Reloading plugins for guild " + _parentHandler.GetGuild().Name);
            string[] toLoad = _enabledPlugins.GetValue().ToArray ();

            foreach (string name in toLoad)
            {
                Type pluginType = PluginLoader.GetPluginType(name);
                if (pluginType == null)
                {
                    Log.Write(Log.Type.WARNING, $"Attempted to instantiate unloaded/unknown plugin type {name}");
                }
                else
                {
                    IPlugin plugin = AssemblyLoader.Instantiate<IPlugin>(pluginType);
                    _activePlugins.Add(plugin);
                }
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Pre-initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.PreInitialize(_parentHandler);
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Initializng plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.Initialize ();
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Post-initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.PostInitialize ();
            }
        }

        public bool AddPlugin(string pluginName)
        {
            Type pluginType = Plugin.Find(PluginLoader.GetPlugins(), pluginName);
            if (pluginType != null)
            {
                string fullName = Plugin.GetVersionedFullName(pluginType);
                if (_enabledPlugins.GetValue().Contains(fullName))
                {
                    throw new ArgumentException("Plugin " + fullName + " is already active.");
                }

                Type[] dependancies = PluginLoader.DependancyTree.GetDependencies(fullName);
                Type[] missing = dependancies.Where(x => !_activePlugins.Any(y => y.GetType() == x)).ToArray();
                if (missing.Length > 0)
                {
                    throw new ArgumentException($"Plugin {fullName} cannot be loaded as it is missing dependancies: {string.Join(",", missing.Select(x => Plugin.GetVersionedFullName(x)))}");
                }

                _enabledPlugins.GetValue().Add(fullName);
                _enabledPlugins.Store();

                ReloadPlugins(); // TODO: Consider looking into hotloading plugins instead of reloading all when a single is added or removed, in case it ever becomes an issue.
                return true;
            }
            throw new ArgumentException("No plugin named '" + pluginName + "' is available to be added.");
        }

        public bool RemovePlugin(string pluginName)
        {
            if (IsPluginActive(pluginName))
            {
                string fullName = Plugin.GetVersionedFullName(Plugin.Find(_activePlugins.Select(x => x.GetType()), pluginName));
                if (Plugin.IsCritical(PluginLoader.GetPluginType(fullName)))
                {
                    throw new ArgumentException("Plugin " + fullName + " is marked critical and cannot be disabled.");
                }

                Type[] dependants = PluginLoader.DependancyTree.GetDependants(fullName);
                Type[] active = dependants.Where(x => _activePlugins.Any(y => y.GetType() == x)).ToArray();
                if (active.Length > 0)
                {
                    throw new ArgumentException($"Plugin {fullName} cannot be unloaded as it has active dependancies: {string.Join(",", active.Select(x => Plugin.GetVersionedFullName(x)))}");
                }

                _enabledPlugins.GetValue().Remove(fullName);
                _enabledPlugins.Store();

                ReloadPlugins();
                return true;
            }
            throw new ArgumentException("No plugin named '" + pluginName + "' is currently active.");
        }

        public bool IsPluginActive (string pluginName) // TODO: Figure out a consistant and simple way to differentiate between plugins. Perhaps a cache of plugin IDs given when their type is loaded? Using strings is at best gonna be a pain in the ass, at worst cause serious issue down the line.
        {
            return Plugin.Find (_activePlugins.Select (x => x.GetType ()), pluginName) != null;
        }
    }
}
