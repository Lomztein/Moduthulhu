using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild.StateManagement;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild
{
    public class PluginManager
    {
        private readonly List<IPlugin> _activePlugins = new List<IPlugin>();
        public int ActiveCount => _activePlugins.Count;

        private readonly GuildHandler _parentHandler;

        private readonly CachedValue<List<string>> _enabledPlugins;
        private readonly List<PluginInitializationException> _initializationExceptions = new List<PluginInitializationException>();

        public event Action OnPrePluginsLoaded;
        public event Action OnPluginsLoaded;
        public event Action<IPlugin> OnPluginLoaded;
        public event Action OnPluginsUnloaded;
        public event Action<IPlugin> OnPluginUnloaded;

        public PluginManager (GuildHandler parent)
        {
            _parentHandler = parent;
            _enabledPlugins = new CachedValue<List<string>>(new DoubleKeyJsonRepository("plugindata"), _parentHandler.GuildId, "EnabledPlugins", () => PluginLoader.GetPlugins().Where (x => PluginLoader.IsDefault (x)).Select (y => Plugin.GetFullName (y)).ToList ());
            _enabledPlugins.Cache();
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
                        if (!IsPluginEnabled(Plugin.GetFullName(pluginType)))
                        {
                            AddPlugin(Plugin.GetFullName(pluginType));
                            Log.Write(Log.Type.PLUGIN, $"Added missing critical {Plugin.GetVersionedFullName(pluginType)} plugin to enabled list.");
                            changed = true;
                        }
                    }
                    catch (ArgumentException exc)
                    {
                        Log.Warning("Tried to add an already enabled plugin to while making super sure critical plugins were enabled.");
                        Log.Exception(exc);
                    }
                }
            }

            if (changed)
            {
                _enabledPlugins.Store();
            }
        }

        private void PurgeDuplicateEnabledPlugins ()
        {
            List<string> value = _enabledPlugins.GetValue ();
            List<string> newValue = new List<string>(value);
            for (int x = 0; x < value.Count; x++)
            {
                for (int y = 0; y < value.Count; y++)
                {
                    if (x == y)
                    {
                        continue;
                    }

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

        public Exception[] GetInitializationExceptions()
        {
            var cur = _initializationExceptions.ToArray();
            ClearInitializationExceptions();
            return cur;
        }

        private void ClearInitializationExceptions() => _initializationExceptions.Clear();

        public IPlugin[] GetActivePlugins() => _activePlugins.ToArray();

        public void ShutdownPlugins ()
        {
            Log.Write(Log.Type.PLUGIN, "Shutting down plugins for handler " + _parentHandler.Name);
            foreach (IPlugin plugin in _activePlugins)
            {
                Log.Write(Log.Type.PLUGIN, "Shutting down plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                plugin.Shutdown();
                OnPluginUnloaded?.Invoke(plugin);
            }
            _activePlugins.Clear();
            OnPluginsUnloaded?.Invoke();
        }

        private static bool ContainsDependencies (IEnumerable<string> enabledList, string pluginName)
        {
            var dependencies = PluginLoader.DependencyTree.GetDependencies(pluginName);
            bool containsDependencies = dependencies.All(x => enabledList.Any(y => y.StartsWith(Plugin.GetFullName(x), StringComparison.Ordinal)));
            return containsDependencies;
        }

        public void LoadPlugins ()
        {
            MakeSuperSureCriticalPluginsAreEnabled();
            PurgeDuplicateEnabledPlugins();

            Log.Write(Log.Type.PLUGIN, "Reloading plugins for guild " + _parentHandler.GetGuild().Name);
            OnPrePluginsLoaded?.Invoke();

            Filter<string> filter = new Filter<string>(x => _enabledPlugins.GetValue().Any (y => NameMatches (x, y)), x => ContainsDependencies (_enabledPlugins.GetValue (), x));
            string[] toLoad = PluginLoader.GetPlugins ().Select (x => Plugin.GetFullName (x)).ToArray ();
            toLoad = filter.FilterModules(toLoad).ToArray ();

            Log.Write(Log.Type.PLUGIN, "Loading plugins: " + string.Join(',', toLoad));

            foreach (string name in toLoad)
            {
                Type pluginType = PluginLoader.GetPlugin(name);
                if (pluginType == null)
                {
                    Log.Write(Log.Type.WARNING, $"Attempted to instantiate unloaded/unknown plugin type {name}");
                }
                else
                {
                    Log.Plugin($"Instantiating plugin '{Plugin.GetVersionedFullName(pluginType)}'.");
                    IPlugin plugin = AssemblyLoader.Instantiate<IPlugin>(pluginType);
                    _activePlugins.Add(plugin);
                }
            }

            bool initError = false;
            foreach (IPlugin plugin in _activePlugins)
            {
                try
                {
                    Log.Write(Log.Type.PLUGIN, "Pre-initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                    plugin.PreInitialize(_parentHandler);
                } catch (Exception exc)
                {
                    ReportInitError("pre-initialization", new PluginInitializationException(Plugin.GetName(plugin.GetType()), exc), plugin, ref initError);
                }
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                try
                {
                    Log.Write(Log.Type.PLUGIN, "Initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                    plugin.Initialize();
                }
                catch (Exception exc)
                {
                    ReportInitError("initialization", new PluginInitializationException(Plugin.GetName(plugin.GetType()), exc), plugin, ref initError);
                }
            }

            foreach (IPlugin plugin in _activePlugins)
            {
                try
                {
                    Log.Write(Log.Type.PLUGIN, "Post-initializing plugin " + Plugin.GetVersionedFullName(plugin.GetType()));
                    plugin.PostInitialize();
                }
                catch (Exception exc)
                {
                    ReportInitError("post-initialization", new PluginInitializationException (Plugin.GetName (plugin.GetType ()), exc), plugin, ref initError);
                }

                OnPluginLoaded?.Invoke(plugin);
            }

            if (initError)
            {
                ReloadPlugins();
            }

            OnPluginsLoaded?.Invoke();
        }

        private void ReportInitError (string step, PluginInitializationException exc, IPlugin plugin, ref bool initErrorFlag)
        {
            Log.Write(Log.Type.WARNING, $"Something went wrong during plugin {step} of plugin '{Plugin.GetName(plugin.GetType())}'. The plugin has been disabled and plugin initialization scheduled to be restarted.");
            Log.Exception(exc);
            _initializationExceptions.Add(exc);
            RemovePlugin(Plugin.GetFullName(plugin.GetType()));
            initErrorFlag = true;
        }

        private static bool NameMatches (string shortName, string longName)
        {
            return longName.StartsWith(shortName, StringComparison.Ordinal);
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

                Type[] dependencies = PluginLoader.DependencyTree.GetDependencies(fullName);
                Type[] missing = dependencies.Where(x => !_enabledPlugins.GetValue ().Any(y => y.StartsWith (Plugin.GetFullName(x), StringComparison.Ordinal))).ToArray();
                if (missing.Length > 0)
                {
                    throw new ArgumentException($"Plugin {fullName} cannot be loaded as it is missing dependencies: {string.Join(",", missing.Select(x => Plugin.GetVersionedFullName(x)))}");
                }

                _enabledPlugins.MutateValue(x => x.Add (fullName));
                return true;
            }
            throw new ArgumentException("No plugin named '" + pluginName + "' is available to be added.");
        }

        public bool RemovePlugin(string pluginName)
        {
            if (IsPluginActive(pluginName))
            {
                string fullName = Plugin.GetFullName(Plugin.Find(_activePlugins.Select(x => x.GetType()), pluginName));
                if (Plugin.IsCritical(PluginLoader.GetPlugin(fullName)))
                {
                    throw new ArgumentException("Plugin " + fullName + " is marked critical and cannot be disabled.");
                }

                Type[] dependants = PluginLoader.DependencyTree.GetDependants(fullName);
                Type[] active = dependants.Where(x => _enabledPlugins.GetValue ().Any(y => y.StartsWith(Plugin.GetFullName(x), StringComparison.Ordinal))).ToArray();
                if (active.Length > 0)
                {
                    throw new ArgumentException($"Plugin {fullName} cannot be unloaded as it has active dependencies: {string.Join(",", active.Select(x => Plugin.GetFullName(x)))}");
                }

                string storedName = _enabledPlugins.GetValue().Find(x => x.StartsWith(fullName, StringComparison.Ordinal));
                _enabledPlugins.MutateValue(x => x.Add (storedName));
                return true;
            }
            throw new ArgumentException("No plugin named '" + pluginName + "' is currently active.");
        }

        public bool IsPluginActive (string pluginName)
        {
            return Plugin.Find (_activePlugins.Select (x => x.GetType ()), pluginName) != null;
        }

        public bool IsPluginEnabled (string pluginName)
        {
            return _enabledPlugins.GetValue().Any(x => x.StartsWith(pluginName, StringComparison.InvariantCulture));
        }

        public JObject RequestUserData (ulong userId)
        {
            JObject data = new JObject();
            foreach (var plugin in _activePlugins)
            {
                string pluginName = Plugin.GetVersionedFullName(plugin.GetType ());
                JToken pluginData = plugin.RequestUserData(userId);
                if (pluginData != null)
                {
                    data.Add(pluginName, pluginData);
                }
            }
            return data;
        }

        public void DeleteUserData(ulong userId) => _activePlugins.ForEach(x => x.DeleteUserData(userId));
    }
}
