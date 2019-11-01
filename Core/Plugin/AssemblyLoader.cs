using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Loader;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Plugin
{
    internal static class AssemblyLoader {

        public static Assembly[] LoadAssemblies (params string[] paths) {
            return paths.Select (x => LoadAssembly (x)).ToArray ();
        }

        public static Assembly LoadAssembly (string path)
        {
            Log.Write(Log.Type.SYSTEM, "Loading assembly file " + Path.GetFileName(path));
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        }

        public static Type[] ExtractTypes<T> (Assembly ass) {

            List<Type> exportedTypes = new List<Type> ();
            Type[] allTypes = ass.GetExportedTypes (); // This is the shittiest line I've ever written.

            foreach (Type type in allTypes) {

                if (type.GetInterfaces().Contains(typeof (T))) {
                    Log.Write (Log.Type.MODULE, $"{typeof(T).Name} type \"{type.Name}\" loaded.");
                    exportedTypes.Add (type);
                }
            }

            return exportedTypes.ToArray ();
        }

        public static Type[] ExtractTypes<T> (params Assembly[] assemblies)
        {
            return assemblies.SelectMany(x => ExtractTypes<T>(x)).ToArray();
        }

        public static Type[] LoadAndExtractTypes<T> (string path)
        {
            return ExtractTypes<T>(LoadAssembly(path));
        }

        public static Type[] LoadAndExtractTypes<T> (string[] path)
        {
            return ExtractTypes<T>(LoadAssemblies(path));
        }

        public static T Instantiate<T>(Type type) where T : class
        {
            return Activator.CreateInstance(type) as T;
        }

        public static T[] LoadAndInstantiate<T>(string path) where T : class
        {
            return LoadAndExtractTypes<T>(path).Select(x => Instantiate<T>(x)).ToArray ();
        }

        public static T[] LoadAndInstantiate<T>(string[] paths) where T : class
        {
            return paths.SelectMany(x => LoadAndInstantiate<T>(x)).ToArray();
        }

        // Might have gotten carried away lol.
    }
}
