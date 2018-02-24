using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System.Reflection;
using System.IO;
using Lomztein.ModularDiscordBot.Core.Bot;
using Lomztein.ModularDiscordBot.Core.Extensions;
using System.Runtime.Loader;
using Lomztein.ModularDiscordBot.Core.Configuration;
using Lomztein.ModularDiscordBot.Core.IO;
using System.Linq;

namespace Lomztein.ModularDiscordBot.Core.Module
{
    public class ModuleHandler {

        public string baseDirectory = AppContext.BaseDirectory + "/Modules/";

        private List<IModule> activeModules = new List<IModule> ();

        private const string CACHE_ENABLE_NAME = "enabled";

        private Dictionary<string, bool> moduleEnableCache;

        private BotClient parentClient;

        public ModuleHandler (BotClient _client, string _baseDirectoy) {
            parentClient = _client;
            baseDirectory = _baseDirectoy;
            LaunchLoad ();
        }

        public async void LaunchLoad() {

            LoadCache ();
            List<IModule> modules = LoadEntireDirectory (baseDirectory);
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

            ConfigureModules ();

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

                if (type.GetInterface ("IModule") == typeof (IModule)) {
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
                toCheck = toCheck.Where (x => !x.Multiserver);
            }

            toCheck = toCheck.Where (x => x.ContainsPrerequisites (toCheck)).ToList ();

            return toCheck.ToList ();
        }

        public void ShutdownModule (IModule module) {
            module.Shutdown ();
            activeModules.Remove (module);
        }

        private void ConfigureModules () {
            foreach (IModule module in activeModules) {
                if (module is IConfigurable configurable) {
                    dynamic dynModule = module;
                    dynModule.Configuration.name = module.CompactizeName ();
                    configurable.ReloadConfiguration ();
                }
            }
        }
    }
}
