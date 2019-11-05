using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Plugin.Framework
{
    public static class Plugin
    {
        private static T GetAttribute<T>(IPlugin plugin) where T : class => plugin.GetType().GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        private static DependencyAttribute[] GetDependancyAttributes(IPlugin plugin) => plugin.GetType().GetCustomAttributes(typeof(DependencyAttribute), false).Cast<DependencyAttribute>().ToArray();
        private static DescriptorAttribute GetDescriptorAttribute(IPlugin plugin) => GetAttribute<DescriptorAttribute>(plugin);
        private static SourceAttribute GetSourceAttribute(IPlugin plugin) => GetAttribute<SourceAttribute>(plugin);

        public static string[] GetDependancy(IPlugin plugin) => GetDependancyAttributes(plugin).Select(x => x.DependencyName).ToArray ();
        public static string GetName(IPlugin plugin) => GetDescriptorAttribute(plugin)?.Name;
        public static string GetAuthor(IPlugin plugin) => GetDescriptorAttribute(plugin)?.Author;
        public static string GetVersion(IPlugin plugin) => GetDescriptorAttribute(plugin)?.Version;
        public static string GetDescription(IPlugin plugin) => GetDescriptorAttribute(plugin)?.Description;

        public static Uri GetAuthorURI(IPlugin plugin) => GetSourceAttribute(plugin)?.AuthorURI;
        public static Uri GetPatchURI(IPlugin plugin) => GetSourceAttribute(plugin)?.PatchURI;
        public static Uri GetProjectURI(IPlugin plugin) => GetSourceAttribute(plugin)?.ProjectURI;

        public static string CompactizeName(IPlugin plugin) => $"{plugin.Author}-{plugin.Name}-{plugin.Version}";

        public static void Log(IPlugin plugin, string text) => Core.Log.Write(Core.Log.GetColor(Core.Log.Type.MODULE), $"{CompactizeName (plugin)} - { plugin.GuildHandler.GetGuild().Name}", text);
    }
}
