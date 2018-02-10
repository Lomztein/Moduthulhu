using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System.Reflection;
using System.IO;
using Lomztein.ModularDiscordBot.Core.Bot;

namespace Lomztein.ModularDiscordBot.Core.Module
{
    public class ModuleHandler {

        public string baseDirectory = AppContext.BaseDirectory + "/Modules/";
        private List<IModule> activeModules = new List<IModule> ();

        private BotClient parentClient;

        public ModuleHandler (BotClient _client, string _baseDirectoy) {
            parentClient = _client;
            baseDirectory = _baseDirectoy;
            LaunchLoad ();
        }

        public void LaunchLoad() {
            List<IModule> modules = LoadEntireDirectory (baseDirectory);

            foreach (IModule module in modules) {
                try {
                    module.ParentModuleHandler = this;
                    module.ParentBotClient = parentClient;
                    module.PreInitialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            foreach (IModule module in modules) {
                try {
                    activeModules.Add (module);
                    module.Initialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            foreach (IModule module in modules) {
                try {
                    CheckModulePrerequisites (module);

                    if (module != null)
                        module.PostInitialize ();
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }
        }

        private List<IModule> LoadEntireDirectory(string path) {
            string [ ] files = Directory.GetFiles (path);
            List<IModule> modules = new List<IModule> ();

            foreach (string file in files) {
                modules.AddRange (LoadAssembly (file));
            }

            return modules;
        }

        public List<IModule> LoadAssembly (string path) {

            Log.Write (Log.Type.SYSTEM, "Loading assembly file " + Path.GetFileName (path));
            List<IModule> exportedModules = new List<IModule> ();
            Assembly moduleAssembly = Assembly.LoadFile (path);
            Type [ ] exportedTypes = moduleAssembly.GetExportedTypes ();

            foreach (Type type in exportedTypes) {

                if (type.GetInterface ("IModule") == typeof (IModule)) {
                    exportedModules.Add (Activator.CreateInstance (type) as IModule);
                    Log.Write (Log.Type.MODULE, "Module loaded: " + type.Name);
                }
            }

            return exportedModules;
        }

        public (string name, string author) DecompressName (string compactName) {
            string [ ] parts = compactName.Split ('.');
            return (parts [ 0 ], parts[1]);
        }

        public bool HasModuleLoaded (string compactName) {
            string name, author;
            (name, author) = DecompressName (compactName);

            return HasModuleLoaded (name, author);
        }

        public bool HasModuleLoaded(string moduleName, string moduleAuthor) {
            return GetModule (moduleName, moduleAuthor) != null;
        }

        public IModule GetModule (string moduleName, string moduleAuthor) {
            return activeModules.Find (x => x.Name == moduleName && x.Author == moduleAuthor);
        }

        private void CheckModulePrerequisites (IModule module) {
            foreach (string required in module.RequiredModules) {
                if (HasModuleLoaded (required)) {
                    Log.Write (Log.Type.EXCEPTION, $"Module {module.Name} cannot load due to missing module prerequisite {required}.");
                    ShutdownModule (module);
                }
            }

            foreach (string recommended in module.RecommnendedModules) {
                if (!HasModuleLoaded (recommended)) {
                    Log.Write (Log.Type.WARNING, $"Module {module.Name} is missing recommended module {recommended}.");
                }
            }
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
    }
}
