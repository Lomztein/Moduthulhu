using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginManager
    {
        private readonly List<IPlugin> _activePlugins = new List<IPlugin>();

        private GuildHandler _parentHandler;

        private CachedValue<List<string>> _enabledPlugins; // TODO: _enabledPlugins currently store full name including version, but it shouldn't include version since that would disable the plugin if the plugin was updated.

        public PluginManager (GuildHandler parent)
        {
            _parentHandler = parent;
            _enabledPlugins = new CachedValue<List<string>>(new IdentityKeyJsonRepository("plugindata"), _parentHandler.GuildId, "EnabledPlugins", () => PluginLoader.GetPlugins().Where (x => PluginLoader.IsStandard (x)).Select (y => Plugin.GetFullName (y)).ToList ());
            _enabledPlugins.Cache();

            MakeSuperSureCriticalPluginsAreEnabled();
            PurgeDuplicateEnabledPlugins();
        }

        private void MakeSuperSureCriticalPluginsAreEnabled ()
        {

            bool changed = false;
            foreach (Type pluginType in PluginLoader.GetPlugins())
            {
                if (Plugin.IsCritical(pluginType))
                {
                    try // One day I promise to stop abusing throw statements, so that we can avoid this.
                    {
                        AddPlugin(Plugin.GetFullName(pluginType));
                        Log.Write(Log.Type.PLUGIN, $"Added missing critical {Plugin.GetVersionedFullName (pluginType)} plugin to enabled list.");
                        changed = true;
                    }
                    catch (ArgumentException) { }
                }
            }

            if (changed) _enabledPlugins.Store();
        }

        private void PurgeDuplicateEnabledPlugins ()
        {
            List<string> value = _enabledPlugins.GetValue ();
            List<string> newValue = new List<string>(value);
            for (int x = 0; x < value.Count; x++)
            {
                for (int y = 0; y < value.Count; y++)
                {
                    if (x == y) continue;

                    string xx = value[x];
                    string yy = value[y];

                    string shortest = xx.Length > yy.Length ? yy : xx;
                    string longest =  xx.Length > yy.Length ? xx : yy;

                    if (longest.StartsWith (shortest, StringComparison.Ordinal))
                    {
                        newValue.Remove(longest);
                        Log.Write(Log.Type.PLUGIN, $"Purged duplicate {shortest} plugin in enabled list.");
                    }
                }
            }
            if (value.Count != newValue.Count)
            {
                _enabledPlugins.SetValue(newValue);
            }
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

        public void LoadPlugins ()
        {
            Log.Write(Log.Type.PLUGIN, "Reloading plugins for guild " + _parentHandler.GetGuild().Name);
            string[] toLoad = _enabledPlugins.GetValue().ToArray ();

            foreach (string name in toLoad)
            {
                Type pluginType = PluginLoader.GetPlugin(name);
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
                Log.Write(Log.Type.PLUGIN, "Initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.Initialize ();
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Post-initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.PostInitialize ();
            }
        }

        public void ReloadPlugins ()
        {
            ShutdownPlugins();
            LoadPlugins();
        }

        public bool AddPlugin(string pluginName)
        {
            Type pluginType = Plugin.Find(PluginLoader.GetPlugins(), pluginName);
            if (pluginType != null)
            {
                string fullName = Plugin.GetFullName(pluginType);
                if (_enabledPlugins.GetValue().Contains(fullName))
                {
                    throw new ArgumentException("Plugin " + fullName + " is already active.");
                }

                Type[] dependancies = PluginLoader.DependancyTree.GetDependencies(fullName);
                Type[] missing = dependancies.Where(x => !_enabledPlugins.GetValue ().Any(y => y.StartsWith (Plugin.GetFullName(x), StringComparison.Ordinal))).ToArray();
                if (missing.Length > 0)
                {
                    throw new ArgumentException($"Plugin {fullName} cannot be loaded as it is missing dependancies: {string.Join(",", missing.Select(x => Plugin.GetVersionedFullName(x)))}");
                }

                _enabledPlugins.GetValue().Add(fullName);
                _enabledPlugins.Store();
                return true;
            }
            throw new ArgumentException("No plugin named '" + pluginName + "' is available to be added.");
        }

        public bool RemovePlugin(string pluginName)
        {
            if (IsPluginActive(pluginName))
            {
                string fullName = Plugin.GetFullName(Plugin.Find(_activePlugins.Select(x => x.GetType()), pluginName));
                if (Plugin.IsCritical(PluginLoader.GetPluginType(fullName)))
                {
                    throw new ArgumentException("Plugin " + fullName + " is marked critical and cannot be disabled.");
                }

                Type[] dependants = PluginLoader.DependancyTree.GetDependants(fullName);
                Type[] active = dependants.Where(x => _enabledPlugins.GetValue ().Any(y => y.StartsWith(Plugin.GetFullName(x), StringComparison.Ordinal))).ToArray();
                if (active.Length > 0)
                {
                    throw new ArgumentException($"Plugin {fullName} cannot be unloaded as it has active dependancies: {string.Join(",", active.Select(x => Plugin.GetFullName(x)))}");
                }

                _enabledPlugins.GetValue().Remove(fullName);
                _enabledPlugins.Store();
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
