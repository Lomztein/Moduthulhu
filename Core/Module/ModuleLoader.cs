using System;
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

        public string BaseDirectory { get => Core.BaseDirectory + "//Modules"; }
        public List<Type> LoadedModuleTypes { get; set; } = new List<Type> ();

        private Bot.Core Core;

        public ModuleLoader (Bot.Core core) {
            Core = core;
            Status.Set ("ModulesPath", BaseDirectory);
        }

        private List<Type> LoadFromDirectory(string path) {
            string [ ] files = Directory.GetFiles (path, "*.dll");
            List<Type> types = new List<Type> ();
            List<Assembly> assemblies = LoadAssemblies (files);

            foreach (Assembly ass in assemblies) {
                types.AddRange (GetModuleTypesFromAssembly (ass));
            }

            return types;
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

        public List<Type> GetModuleTypesFromAssembly (Assembly ass) {

            List<Type> exportedTypes = new List<Type> ();
            Type[] allTypes = ass.GetExportedTypes (); // This is the shittiest line I've ever written.

            foreach (Type type in allTypes) {

                if (type.GetInterfaces().Contains (typeof (IModule))) {
                    Log.Write (Log.Type.MODULE, $"Module type \"{type.Name}\" loaded.");
                    exportedTypes.Add (type);
                }
            }

            return exportedTypes;
        }
    }
}
