using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Cross;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Bot.Client.Sharding
{
    public class ModuleContainer
    {
        private Shard ParentShard { get; set; }
        private ModuleLoader Loader { get => ParentShard.Core.ModuleLoader; }

        public List<IModule> Modules { get; private set; }


        internal void InstantiateModules () {

            ModuleFilter filter = new ModuleFilter (
                x => IsModuleEnabled (x.CompactizeName ())
                );

            Log.Write (Log.Type.MODULE, $"Instantiating modules for {ParentShard}..");
            Modules = Loader.InstantiateAll ().ToList ();
            Modules = filter.FilterModules (Modules).ToList ();
        }

        internal void InitializeModules () {
            
            // Pre-initialize
            foreach (IModule module in Modules) {

                module.ParentShard = ParentShard;
                module.ParentContainer = this;

                module.Log ("Pre-initializing..");
                module.PreInitialize ();
            }

            AutoConfigureModules ();

            // Initialize
            foreach (IModule module in Modules) {
                module.Log ("Initializing..");
                module.Initialize ();
            }

            // Post-initialize
            foreach (IModule module in Modules) {
                module.Log ("Post-initializing..");
                module.PostInitialize ();
            }

        }

        public ModuleContainer (Shard parentShard) {
            ParentShard = parentShard;
        }

        public T GetModule<T>() {
            IModule module = Modules.Find (x => x is T);
            if (module == null)
                return default (T);
            else
                return (T)module;
        }

        public bool IsModuleLoaded<T>() {
            return GetModule<T> () != null;
        }

        private bool IsModuleEnabled(string compactName) {
            return true;
        }

        internal void ShutdownModule(IModule module) {
            Log.Write (Log.Type.MODULE, "Shutting down module: " + module.CompactizeName ());
            module.Shutdown ();
        }

        private void ClearModuleCache() => Modules = new List<IModule> ();

        internal void ShutdownAllModules() {
            Log.Write (Log.Type.MODULE, "Shutting down all modules..");
            foreach (IModule module in Modules)
                ShutdownModule (module);
            ClearModuleCache ();
        }

        /// <summary>
        /// Returns the first module that matches the given string even remotely.
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public IModule FuzzySearchModule(string searchString) {
            foreach (IModule module in Modules) {
                string upperedName = module.Name.ToUpper ();
                string upperedInput = searchString.ToUpper ();

                if (upperedName.Contains (upperedInput))
                    return module;
            }
            return null;
        }

        internal void AutoConfigureModules() {
            List<SocketGuild> allGuilds = ParentShard.Guilds.ToList ();

            foreach (IModule module in Modules) {
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
