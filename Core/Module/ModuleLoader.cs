﻿using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System.Reflection;
using System.IO;
using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.Extensions;
using System.Runtime.Loader;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.IO;
using System.Linq;
using Discord.WebSocket;
using Lomztein.Moduthulhu.Cross;

namespace Lomztein.Moduthulhu.Core.Module
{
    public class ModuleLoader {

        // This class could use a refactoring, it's gotten a bit messy.
        // TODO: Create a proper dependancy tree and order initialization based on that, as well as allowing for better on-the-fly swapping.
        // TODO: Figure out much more robust error handling for modules.
        // TODO: Rework the flow of this class in order to be much cleaner.

        public readonly string baseDirectory = AppContext.BaseDirectory + "/Modules/";

        public List<IModule> activeModules = new List<IModule> ();

        private const string CACHE_ENABLE_NAME = "enabled";

        private Dictionary<string, bool> moduleEnableCache;

        private Bot.Core parentClient;

        public ModuleLoader (Bot.Core _client, string _baseDirectoy) {
            parentClient = _client;
            baseDirectory = _baseDirectoy;
            Status.Set ("ModulesPath", baseDirectory);
            LoadModuleFolder ();
        }

        public void ReloadModules () {
            ShutdownAllModules ();
            LoadModuleFolder ();
        }

        private async void LoadModuleFolder() {

            LoadCache ();
            List<IModule> modules = LoadEntireDirectory(baseDirectory);
            await parentClient.AwaitFullBoot ();
            activeModules = FilterEnabledModules (modules);
            
            Log.Write (Log.Type.MODULE, "Pre-initializing modules.");
            foreach (IModule module in activeModules) {
                try {
                    module.ParentModuleHandler = this;
                    module.ParentBotClient = parentClient;

                    module.PreInitialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            AutoConfigureModules ();

            Log.Write (Log.Type.MODULE, "Initializing modules.");
            foreach (IModule module in activeModules) {
                try {
                    module.Initialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            Log.Write (Log.Type.MODULE, "Post-initializing modules.");
            foreach (IModule module in activeModules) {
                try {
                     module.PostInitialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            SaveCache ();
        }

        private List<IModule> LoadEntireDirectory(string path) {
            string [ ] files = Directory.GetFiles (path, "*.dll");
            List<IModule> modules = new List<IModule> ();
            List<Assembly> assemblies = LoadAssemblies (files);

            foreach (Assembly ass in assemblies) {
                modules.AddRange (GetModulesFromAssembly (ass));
            }

            return modules;
        }

        private List<Assembly> LoadAssemblies (params string[] paths) {
            List<Assembly> result = new List<Assembly> ();
            foreach (string path in paths) {
                Log.Write (Log.Type.SYSTEM, "Loading assembly file " + Path.GetFileName (path));
                Assembly ass = AssemblyLoadContext.Default.LoadFromAssemblyPath (path);
                result.Add (ass);
            }

            return result;
        }

        public List<IModule> GetModulesFromAssembly (Assembly ass) {

            List<IModule> exportedModules = new List<IModule> ();

            Type[] exportedTypes = ass.GetExportedTypes (); // This is the shittiest line I've ever written.

            foreach (Type type in exportedTypes) {

                if (type.GetInterfaces().Contains (typeof (IModule))) {
                    IModule module = Activator.CreateInstance (type) as IModule;
                    Log.Write (Log.Type.MODULE, "Module loaded: " + module.CompactizeName ());
                    exportedModules.Add (module);
                }
            }

            return exportedModules;
        }

        public bool IsModuleLoaded<T> () {
            return GetModule<T>() != null;
        }

        public IModule GetModule (string moduleName, string moduleAuthor) {
            return activeModules.Find (x => x.Name == moduleName && x.Author == moduleAuthor);
        }

        /// <summary>
        /// Returns a copy of the internal list of active modules as an array, so that it cannot be modified from the outside.
        /// </summary>
        /// <returns></returns>
        public IModule[] GetActiveModules() => activeModules.ToArray ();

        public T GetModule<T>() {
            IModule module = activeModules.Find (x => x is T);
            if (module == null)
                return default (T);
            else
                return (T)module;
        }

        public void LoadCache () {
            moduleEnableCache = JSONSerialization.DeserializeFile<Dictionary<string, bool>> (baseDirectory + CACHE_ENABLE_NAME);
            if (moduleEnableCache == null)
                moduleEnableCache = new Dictionary<string, bool> ();
        }

        public void SaveCache () {
            JSONSerialization.SerializeObject (moduleEnableCache, baseDirectory + CACHE_ENABLE_NAME, true);
        }

        private bool IsModuleEnabled (string compactName) {
            if (!moduleEnableCache.ContainsKey (compactName))
                moduleEnableCache.Add (compactName, true);
            return moduleEnableCache [ compactName ];
        }

        private List<IModule> FilterEnabledModules (IEnumerable<IModule> toCheck) {
            toCheck = toCheck.Where (x => IsModuleEnabled (x.CompactizeName ())).ToList ();
            if (parentClient.IsMultiserver ()) {
                toCheck = toCheck.Where (x => x.Multiserver);
            }

            var clone = toCheck.ToList ();
            toCheck = toCheck.Where (x => x.ContainsPrerequisites (clone)).ToList ();

            return toCheck.ToList ();
        }

        public void ShutdownModule (IModule module) {
            Log.Write (Log.Type.MODULE, "Shutting down module: " + module.CompactizeName ());
            module.Shutdown ();
        }

        public void ClearModuleCache () => activeModules = new List<IModule> ();

        public void ShutdownAllModules () {
            Log.Write (Log.Type.MODULE, "Shutting down all modules..");
            foreach (IModule module in activeModules)
                ShutdownModule (module);
            ClearModuleCache ();
        }

        /// <summary>
        /// Returns the first module that matches the given string even remotely.
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public IModule FuzzySearchModule (string searchString) {
            foreach (IModule module in activeModules) {
                string upperedName = module.Name.ToUpper ();
                string upperedInput = searchString.ToUpper ();

                if (upperedName.Contains (upperedInput))
                    return module;
            }
            return null;
        }

        public void AutoConfigureModules () {
            List<SocketGuild> allGuilds = parentClient.DiscordClient.Guilds.ToList ();

            foreach (IModule module in activeModules) {
                if (module is IConfigurable configurable) {
                    dynamic dynModule = module;
                    dynModule.Configuration.Name = module.CompactizeName ();
                    dynModule.Configuration.Load ();
                    configurable.AutoConfigure (allGuilds);
                    dynModule.Configuration.Save ();
                }
            }
        }
    }
}