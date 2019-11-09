using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Plugins;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    public class PluginManagerCommands : PluginCommandSet<PluginManagerPlugin>
    {
        public PluginManagerCommands ()
        {
            Name = "plugins";
            Description = "Manage active plugins.";
            Category = AdditionalCategories.Management;

            commandsInSet = new List<ICommand>()
            {
                new AddCommand (),
                new RemoveCommand (),
                new ActiveCommand (),
                new AvailableCommand (),
                new InfoCommand (),
            };
        }

        private class AddCommand : PluginCommand<PluginManagerPlugin>
        {
            public AddCommand ()
            {
                Name = "enable";
                Description = "Add a plugin.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(void), "Add a new plugin from the list of available plugins.")]
            public Task<Result> Execute(CommandMetadata metadata, string pluginName)
            {
                ParentPlugin.AddPlugin(pluginName);
                return TaskResult(null, $"Added plugin {Plugin.GetFullName(PluginLoader.GetPluginType(pluginName))}.");
            }
        }

        private class RemoveCommand : PluginCommand<PluginManagerPlugin>
        {
            public RemoveCommand()
            {
                Name = "disable";
                Description = "Remove a plugin.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(void), "Remove a plugin from currently active plugins.")]
            public Task<Result> Execute(CommandMetadata metadata, string pluginName)
            {
                ParentPlugin.RemovePlugin(pluginName);
                return TaskResult(null, $"Removed plugin {Plugin.GetFullName(PluginLoader.GetPluginType(pluginName))}.");
            }
        }

        private class ActiveCommand : PluginCommand<PluginManagerPlugin>
        {
            public ActiveCommand()
            {
                Name = "active";
                Description = "Active plugins.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(Embed), "Display all currently active plugins on this server.")]
            public Task<Result> Execute(CommandMetadata metadata)
            {
                return TaskResult(GetModuleListEmbed(ParentPlugin.GetActivePlugins ().Select(x => x.GetType()), "All active plugins.", "A list of all currently active plugins on this server."), null);
            }
        }

        private class AvailableCommand : PluginCommand<PluginManagerPlugin>
        {
            public AvailableCommand()
            {
                Name = "available";
                Description = "Available plugins.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(Embed), "Display all available plugins.")]
            public Task<Result> Execute(CommandMetadata metadata)
            {
                return TaskResult(GetModuleListEmbed(ParentPlugin.GetAvailablePlugins(), "All active plugins.", "A list of all currently active plugins on this server."), null);
            }
        }

        private class InfoCommand : PluginCommand<PluginManagerPlugin>
        {
            public InfoCommand()
            {
                Name = "info";
                Description = "Plugin information.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(Embed), "Display information about a specific plugin.")]
            public Task<Result> Execute(CommandMetadata metadata, string pluginName)
            {
                Type pluginType = Plugin.Find(PluginLoader.GetPlugins (), pluginName);
                return TaskResult(GetModuleEmbed(pluginType), null);
            }
        }

        public static Embed GetModuleEmbed(Type moduleType)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(Plugin.GetName(moduleType))
                .WithDescription(Plugin.GetDescription(moduleType))
                .WithAuthor("Plugin Information")
                .WithFooter("Created by " + Plugin.GetAuthor(moduleType) + " - " + Plugin.GetAuthorURI(moduleType));

            AddDependanciesInline("Prerequisite Plugin", PluginLoader.DependancyTree.GetDependencies(Plugin.GetVersionedFullName(moduleType)).Select(x => Plugin.GetVersionedFullName(x)).ToArray());

            void AddDependanciesInline(string header, string[] dependancies)
            { // Never did I ever say I knew how to spell.

                if (dependancies.Length > 0)
                {

                    string content = "";
                    foreach (string dep in dependancies)
                        content += dep + "\n";

                    builder.AddField(header, content);
                }

            }

            return builder.Build();
        }

        public static Embed GetModuleListEmbed(IEnumerable<Type> pluginsTypes, string title, string description)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor("Plugin Information")
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(pluginsTypes.Count () + " plugins.");

            foreach (Type plugin in pluginsTypes)
            {
                builder.AddField(Plugin.GetVersionedFullName (plugin), Plugin.GetDescription (plugin));
            }

            return builder.Build();
        }
    }
}
