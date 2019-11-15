using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Core.Plugins
{
    internal class PluginDependancyTree
    {
        private readonly Branch[] _branches;

        internal PluginDependancyTree (params Type[] types) {
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
                DependencyAttribute[] dependancyAttributes = branch.Plugin.GetCustomAttributes (typeof (DependencyAttribute), false).Cast<DependencyAttribute>().ToArray ();
                string branchName = Framework.Plugin.GetVersionedFullName(branch.Plugin);
                List<Branch> dependancyBranches = new List<Branch>();

                foreach (DependencyAttribute dependancyAttribute in dependancyAttributes)
                {
                    Branch dependancyBranch = GetBranch(dependancyAttribute.DependencyName);
                    if (dependancyBranch == null)
                    {
                        Log.Write(Log.Type.CRITICAL, $"Missing dependancy {dependancyAttribute.DependencyName} for plugin {branchName}!");
                    }
                    else
                    {
                        string dependancyVersion = Framework.Plugin.GetVersion(dependancyBranch.Plugin);
                        dependancyBranches.Add(dependancyBranch);

                        Log.Write(Log.Type.PLUGIN, $"Plugin {branchName} linked to dependancy {Framework.Plugin.GetVersionedFullName(dependancyBranch.Plugin)}.");
                        if (dependancyAttribute.DesiredVersion != dependancyVersion)
                        {
                            string targetVersion = dependancyAttribute.DesiredVersion;
                            Log.Write(Log.Type.WARNING, $"Plugin {branchName} targets version {targetVersion} of {dependancyBranch.Plugin.Name}, but version {dependancyVersion} is installed instead. This may cause issues.");
                        }
                    }
                }

                branch.SetDependancies (dependancyBranches.ToArray ());
            }
        }

        // This is unlikely to be the most, if at all an effecient method, but it seems to work just fine.
        public IEnumerable<Type> Order (IEnumerable<Type> pluginTypes) {

            List<Type> toOrder = pluginTypes.ToList ();
            List<Type> allSoFar = new List<Type> ();

            int currentIndex = 0;
            while (toOrder.Count != 0) {

                if (currentIndex > toOrder.Count - 1) {
                    Log.Write (Log.Type.CRITICAL, $"Plugins {toOrder.Select (x => x.Name).Singlify ()} are missing dependencies, they have been excluded in the sort.");
                    break;
                }

                Type currentType = toOrder[currentIndex];
                Branch currentBranch = GetBranch (Framework.Plugin.GetVersionedFullName (currentType));
                
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
                string oName = Framework.Plugin.GetVersionedFullName(other.Plugin);
                if (oName.StartsWith(pluginName, StringComparison.Ordinal))
                {
                    branch = other;
                    break;
                }
            }
            if (branch == null)
                Log.Write (Log.Type.CRITICAL, $"Plugins type {pluginName} cannot be found, perhaps it is missing in the Modules folder.");
            return branch;
        }

        public bool Matches (string pluginName, string pluginVersion, string dependancyName, string dependancyVersion)
        {
            if (pluginName == dependancyName)
            {
                if (pluginVersion != dependancyVersion)
                {
                    Log.Write(Log.Type.WARNING, $"Dependancy plugin {dependancyName} is loaded, but it is a different version than the targeted version. This may cause issues.");
                }

                return true;
            }
            else return false;
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

            internal void SetDependancies (params Branch[] dependancies) {
                Dependencies = dependancies;
            }

        }
    }
}
