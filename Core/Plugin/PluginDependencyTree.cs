using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Core.Plugins
{
    internal class PluginDependencyTree
    {
        private readonly Branch[] _branches;

        internal PluginDependencyTree (params Type[] types) {
            _branches = ConstructBranches (types);
            ConnectBranches (_branches);
        }

        private Branch[] ConstructBranches (Type[] types) {
            Branch[] branches = new Branch[types.Length];
            
            for (int i = 0; i < branches.Length; i++) {
                branches[i] = new Branch (types[i]);
            }

            return branches;
        }

        private void ConnectBranches (Branch[] branches) {
            foreach (Branch branch in branches) {
                DependencyAttribute[] dependencyAttributes = branch.Plugin.GetCustomAttributes (typeof (DependencyAttribute), false).Cast<DependencyAttribute>().ToArray ();
                string branchName = Plugin.GetVersionedFullName(branch.Plugin);
                List<Branch> dependencyBranches = new List<Branch>();

                foreach (DependencyAttribute dependencyAttribute in dependencyAttributes)
                {
                    Branch dependencyBranch = GetBranch(dependencyAttribute.DependencyName);
                    if (dependencyBranch == null)
                    {
                        Log.Critical($"Missing dependency {dependencyAttribute.DependencyName} for plugin {branchName}!");
                    }
                    else
                    {
                        string dependencyVersion = Plugin.GetVersion(dependencyBranch.Plugin);
                        dependencyBranches.Add(dependencyBranch);

                        Log.Plugin($"Plugin {branchName} linked to dependency {Plugin.GetVersionedFullName(dependencyBranch.Plugin)}.");
                        if (dependencyAttribute.DesiredVersion != dependencyVersion)
                        {
                            string targetVersion = dependencyAttribute.DesiredVersion;
                            Log.Warning($"Plugin {branchName} targets version {targetVersion} of {dependencyBranch.Plugin.Name}, but version {dependencyVersion} is installed instead. This may cause issues.");
                        }
                    }
                }

                branch.SetDependencies (dependencyBranches.ToArray ());
            }
        }

        // This is unlikely to be the most, if at all an effecient method, but it seems to work just fine.
        public IEnumerable<Type> Order (IEnumerable<Type> pluginTypes) {

            List<Type> toOrder = pluginTypes.ToList ();
            List<Type> allSoFar = new List<Type> ();

            int currentIndex = 0;
            while (toOrder.Count != 0) {

                if (currentIndex > toOrder.Count - 1) {
                    Log.Critical ($"Plugins {string.Join (", ", toOrder.Select (x => x.Name))} are missing dependencies, they have been excluded in the sort.");
                    break;
                }

                Type currentType = toOrder[currentIndex];
                Branch currentBranch = GetBranch (Plugin.GetVersionedFullName (currentType));
                
                if (currentBranch.Dependencies.Length == 0 || currentBranch.Dependencies.All (x => x != null && allSoFar.Contains (x.Plugin))) {
                    allSoFar.Add (currentType);
                    toOrder.RemoveAt (currentIndex);
                    currentIndex = 0;
                }else {
                    currentIndex++;
                }

            }

            return allSoFar;
        }

        private Branch GetBranch (string pluginName) {
            Branch branch = null;
            
            foreach (Branch other in _branches)
            {
                string oName = Plugin.GetVersionedFullName(other.Plugin);
                if (oName.StartsWith(pluginName, StringComparison.Ordinal))
                {
                    branch = other;
                    break;
                }
            }
            if (branch == null)
            {
                Log.Critical($"Plugins type {pluginName} cannot be found, perhaps it is missing in the Plugins folder.");
            }
            return branch;
        }

        public static bool Matches (string pluginName, string pluginVersion, string dependencyName, string dependencyVersion)
        {
            if (pluginName == dependencyName)
            {
                if (pluginVersion != dependencyVersion)
                {
                    Log.Warning($"Dependency plugin {dependencyName} is loaded, but it is a different version than the targeted version. This may cause issues.");
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public Type[] GetDependencies (string pluginName) {
            return GetBranch (pluginName).Dependencies.Select (x => x.Plugin).ToArray ();
        }

        public Type[] GetDependants (string pluginName)
        {
            Branch branch = GetBranch(pluginName);
            return _branches.Where(x => x.Dependencies.Contains(branch)).Select (x => x.Plugin).ToArray();
        }



        internal class Branch {

            internal Type Plugin { get; set; }
            internal Branch[] Dependencies { get; set; }

            internal Branch(Type module) {
                Plugin = module;
            }

            internal void SetDependencies (params Branch[] dependencies) {
                Dependencies = dependencies;
            }

        }
    }
}
