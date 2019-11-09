using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public static class Plugin
    {
        private static T GetAttribute<T>(Type plugin) where T : class => plugin.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        private static DependencyAttribute[] GetDependancyAttributes(Type plugin) => plugin.GetType().GetCustomAttributes(typeof(DependencyAttribute), false).Cast<DependencyAttribute>().ToArray();
        private static DescriptorAttribute GetDescriptorAttribute(Type plugin) => GetAttribute<DescriptorAttribute>(plugin);
        private static SourceAttribute GetSourceAttribute(Type plugin) => GetAttribute<SourceAttribute>(plugin);

        public static string[] GetDependancy(Type plugin) => GetDependancyAttributes(plugin).Select(x => x.DependencyName).ToArray ();
        public static string GetName(Type plugin) => GetDescriptorAttribute(plugin)?.Name;
        public static string GetAuthor(Type plugin) => GetDescriptorAttribute(plugin)?.Author;
        public static string GetVersion(Type plugin) => GetDescriptorAttribute(plugin)?.Version;
        public static string GetDescription(Type plugin) => GetDescriptorAttribute(plugin)?.Description;
        public static int GetID(Type plugin) => PluginLoader.GetID (plugin);
        public static bool IsCritical(Type plugin) => GetAttribute<CriticalAttribute>(plugin) != null;

        public static Uri GetAuthorURI(Type plugin) => new Uri (GetSourceAttribute(plugin)?.AuthorURI);
        public static Uri GetPatchURI(Type plugin) => new Uri (GetSourceAttribute(plugin)?.PatchURI);
        public static Uri GetProjectURI(Type plugin) => new Uri (GetSourceAttribute(plugin)?.ProjectURI);

        public static string GetVersionedFullName (string author, string name, string version) => $"{author}-{name}-{version}";
        public static string GetVersionedFullName (Type plugin) => $"{GetAuthor (plugin)}-{GetName (plugin)}-{GetVersion (plugin)}";
        public static string GetFullName (string author, string name) => $"{author}-{name}";
        public static string GetFullName (Type plugin) => $"{GetAuthor(plugin)}-{GetName (plugin)}";

        public static void Log(IPlugin plugin, string text) => Core.Log.Write(Core.Log.GetColor(Core.Log.Type.PLUGIN), $"{GetVersionedFullName (plugin.GetType ())} - { plugin.GuildHandler.Name}", text);

        public static Type Find (IEnumerable<Type> plugins, string search)
        {
            List<Type> applicable = new List<Type>();

            // Find by name. Return if only one is found.
            applicable = plugins.Where(x => GetName(x).ToUpperInvariant () == search.ToUpperInvariant()).ToList ();
            if (applicable.Count == 1) return applicable.First();
            applicable.Clear();

            // Find by full name. Return if only one is found.
            applicable = plugins.Where(x => GetFullName(x).ToUpperInvariant() == search.ToUpperInvariant()).ToList();
            if (applicable.Count == 1) return applicable.First();
            applicable.Clear();

            // Find by versioned full name. Return if only one is found.
            applicable = plugins.Where(x => GetVersionedFullName(x).ToUpperInvariant() == search.ToUpperInvariant()).ToList();
            if (applicable.Count == 1) return applicable.First();
            applicable.Clear();

            return null;
        }
    }
}
