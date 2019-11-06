using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
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

        public static Uri GetAuthorURI(Type plugin) => new Uri (GetSourceAttribute(plugin)?.AuthorURI);
        public static Uri GetPatchURI(Type plugin) => new Uri (GetSourceAttribute(plugin)?.PatchURI);
        public static Uri GetProjectURI(Type plugin) => new Uri (GetSourceAttribute(plugin)?.ProjectURI);

        public static string CompactizeName(Type plugin) => $"{GetAuthor (plugin)}-{GetName (plugin)}-{GetVersion (plugin)}";

        public static void Log(IPlugin plugin, string text) => Core.Log.Write(Core.Log.GetColor(Core.Log.Type.PLUGIN), $"{CompactizeName (plugin.GetType ())} - { plugin.GuildHandler.Name}", text);
    }
}
