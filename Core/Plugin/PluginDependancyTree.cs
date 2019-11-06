using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Core.Extensions;

namespace Lomztein.Moduthulhu.Core.Plugin
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

                DependencyAttribute[] dependencies = branch.Plugin.GetCustomAttributes (typeof (DependencyAttribute), false).Cast<DependencyAttribute>().ToArray ();
                string[] dependencyModules = dependencies.Select (x => x.DependencyName).ToArray ();
                branch.SetDependancies (dependencyModules.Select (x => GetBranch (x)).ToArray ());

            }

        }

        // This is unlikely to be the most, if at all an effecient method, but it seems to work just fine.
        public IEnumerable<Type> Order (IEnumerable<Type> moduleTypes) {

            List<Type> toOrder = moduleTypes.ToList ();
            List<Type> allSoFar = new List<Type> ();

            int currentIndex = 0;
            while (toOrder.Count != 0) {

                if (currentIndex > toOrder.Count - 1) {
                    Log.Write (Log.Type.CRITICAL, $"Modules {toOrder.Select (x => x.Name).Singlify ()} are missing dependencies, they have been excluded in the sort.");
                    break;
                }

                Type currentType = toOrder[currentIndex];
                Branch currentBranch = GetBranch (currentType.Name);
                
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

        private Branch GetBranch (string moduleType) {
            Branch branch = _branches.FirstOrDefault (x => x.Plugin.Name == moduleType);
            if (branch == null)
                Log.Write (Log.Type.CRITICAL, $"Module type {moduleType} cannot be found, perhaps it is missing in the Modules folder.");
            return branch;
        }

        public Type[] GetDependencies (Type moduleType) {
            return GetBranch (moduleType.Name).Dependencies.Select (x => x.Plugin).ToArray ();
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
