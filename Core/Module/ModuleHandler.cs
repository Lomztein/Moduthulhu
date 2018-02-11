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
        private Dictionary<string, bool> moduleEnableCache;

        private BotClient parentClient;

        public ModuleHandler (BotClient _client, string _baseDirectoy) {
            parentClient = _client;
            baseDirectory = _baseDirectoy;
            LaunchLoad ();
        }
        
        public async void LaunchLoad() {

            LoadEnableCache ();
            List<IModule> modules = LoadEntireDirectory (baseDirectory);
            activeModules = FilterEnabledModules (modules);
            SaveEnableCache ();

            Log.Write (Log.Type.MODULE, "Pre-initializing modules.");
            foreach (IModule module in modules) {
                try {
                    module.ParentModuleHandler = this;
                    module.ParentBotClient = parentClient;

                    module.PreInitialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            await parentClient.AwaitFullBoot ();
            ConfigureModules ();

            Log.Write (Log.Type.MODULE, "Initializing modules.");
            foreach (IModule module in modules) {
                try {
                    module.Initialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            Log.Write (Log.Type.MODULE, "Post-initializing modules.");
            foreach (IModule module in modules) {
                try {
                     module.PostInitialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }
        }

        private List<IModule> LoadEntireDirectory(string path) {
            string [ ] files = Directory.GetFiles (path, "*.dll");
            List<IModule> modules = new List<IModule> ();

            foreach (string file in files) {
                modules.AddRange (LoadAssembly (file));
            }

            return modules;
        }

        public List<IModule> LoadAssembly (string path) {

            Log.Write (Log.Type.SYSTEM, "Loading assembly file " + Path.GetFileName (path));
            List<IModule> exportedModules = new List<IModule> ();

            Assembly moduleAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath (path);
            Type [ ] exportedTypes = moduleAssembly.GetExportedTypes ();

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
            IModule module = activeModules.Find (x => x.GetType () == typeof (T));
            if (module == null)
                return default (T);
            else
                return (T)module;
        }

        public void LoadEnableCache () {
            moduleEnableCache = JSONSerialization.DeserializeFile<Dictionary<string, bool>> (baseDirectory + "cache");
            if (moduleEnableCache == null)
                moduleEnableCache = new Dictionary<string, bool> ();
        }

        public void SaveEnableCache () {
            JSONSerialization.SerializeObject (moduleEnableCache, baseDirectory + "cache", true);
        }

        private bool IsModuleEnabled (string compactName) {
            if (!moduleEnableCache.ContainsKey (compactName))
                moduleEnableCache.Add (compactName, true);
            return moduleEnableCache [ compactName ];
        }

        private List<IModule> FilterEnabledModules (IEnumerable<IModule> toCheck) {
            toCheck = toCheck.Where (x => IsModuleEnabled (x.CompactizeName ())).ToList ();
            toCheck = toCheck.Where (x => x.ContainsPrerequisites (toCheck)).ToList ();

            return toCheck.ToList ();
        }

        private void InitializeModule (IModule module) {
            module.Initialize ();
            activeModules.Add (module);
            module.PostInitialize ();
        }

        private void ShutdownModule (IModule module) {
            module.Shutdown ();
            activeModules.Remove (module);
        }

        private void ConfigureModules () {
            foreach (IModule module in activeModules)
                (module as IConfigurable)?.Configure ();
        }
    }
}
