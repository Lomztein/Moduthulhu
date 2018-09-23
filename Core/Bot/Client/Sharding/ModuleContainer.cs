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
        internal Shard ParentShard { get; private set; }
        public ModuleLoader Loader { get => ParentShard.Core.ModuleLoader; }
        public List<IModule> Modules { get; private set; }

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

        private List<IModule> FilterEnabledModules(IEnumerable<IModule> toCheck) {
            toCheck = toCheck.Where (x => IsModuleEnabled (x.CompactizeName ())).ToList ();

            var clone = toCheck.ToList ();
            toCheck = toCheck.Where (x => x.ContainsPrerequisites (clone)).ToList ();

            return toCheck.ToList ();
        }

        public void ShutdownModule(IModule module) {
            Log.Write (Log.Type.MODULE, "Shutting down module: " + module.CompactizeName ());
            module.Shutdown ();
        }

        public void ClearModuleCache() => Modules = new List<IModule> ();

        public void ShutdownAllModules() {
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

        public void AutoConfigureModules() {
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
