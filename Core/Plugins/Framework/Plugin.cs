using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugins.Framework
{
    public static class Plugin
    {
        private static T GetAttribute<T>(Type plugin) where T : class => plugin.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        private static DependencyAttribute[] GetDependencyAttributes(Type plugin) => plugin.GetCustomAttributes(typeof(DependencyAttribute), false).Cast<DependencyAttribute>().ToArray();
        private static DescriptorAttribute GetDescriptorAttribute(Type plugin) => GetAttribute<DescriptorAttribute>(plugin);
        private static SourceAttribute GetSourceAttribute(Type plugin) => GetAttribute<SourceAttribute>(plugin);
        private static GDPRAttribute GetGDPRAttribute(Type plugin) => GetAttribute<GDPRAttribute>(plugin);

        public static string[] GetDependency(Type plugin) => GetDependencyAttributes(plugin).Select(x => x.DependencyName).ToArray();
        public static string GetName(Type plugin) => GetDescriptorAttribute(plugin)?.Name;
        public static string GetAuthor(Type plugin) => GetDescriptorAttribute(plugin)?.Author;
        public static string GetVersion(Type plugin) => GetDescriptorAttribute(plugin)?.Version;
        public static string GetDescription(Type plugin) => GetDescriptorAttribute(plugin)?.Description;
        public static bool IsCritical(Type plugin) => GetAttribute<CriticalAttribute>(plugin) != null;

        public static Uri GetAuthorURI(Type plugin) => Uri.TryCreate(GetSourceAttribute(plugin)?.AuthorURI, UriKind.Absolute, out Uri uri) ? uri : null;
        public static Uri GetPatchURI(Type plugin) => Uri.TryCreate(GetSourceAttribute(plugin)?.PatchURI, UriKind.Absolute, out Uri uri) ? uri : null;
        public static Uri GetProjectURI(Type plugin) => Uri.TryCreate(GetSourceAttribute(plugin)?.ProjectURI, UriKind.Absolute, out Uri uri) ? uri : null;

        public static GDPRCompliance? GetGDPRCompliance(Type plugin) => GetGDPRAttribute (plugin)?.Compliance;
        public static string[] GetGDPRNotes(Type plugin) => GetGDPRAttribute(plugin)?.Notes;

        public static string GetVersionedFullName (string author, string name, string version) => $"{author}-{name}-{version}";
        public static string GetVersionedFullName (Type plugin) => $"{GetAuthor (plugin)}-{GetName (plugin)}-{GetVersion (plugin)}";
        public static string GetFullName (string author, string name) => $"{author}-{name}";
        public static string GetFullName (Type plugin) => $"{GetAuthor(plugin)}-{GetName (plugin)}";

        public static void Log(IPlugin plugin, string text) => Core.Log.Plugin($"[{GetVersionedFullName (plugin.GetType ())} - {plugin.GuildHandler.Name}] {text}");

        public static Type Find (IEnumerable<Type> plugins, string search)
        {
            List<Type> applicable;

            // Find by name. Return if only one is found.
            applicable = plugins.Where(x => GetName(x).ToUpperInvariant () == search.ToUpperInvariant()).ToList ();
            if (applicable.Count == 1)
            {
                return applicable.First();
            }
            applicable.Clear();

            // Find by full name. Return if only one is found.
            applicable = plugins.Where(x => GetFullName(x).ToUpperInvariant() == search.ToUpperInvariant()).ToList();
            if (applicable.Count == 1)
            {
                return applicable.First();
            }
            applicable.Clear();

            // Find by versioned full name. Return if only one is found.
            applicable = plugins.Where(x => GetVersionedFullName(x).ToUpperInvariant() == search.ToUpperInvariant()).ToList();
            if (applicable.Count == 1)
            {
                return applicable.First();
            }
            applicable.Clear();

            return null;
        }
    }
}
