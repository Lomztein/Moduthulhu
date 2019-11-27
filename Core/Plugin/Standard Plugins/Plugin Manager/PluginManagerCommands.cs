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

            commandsInSet = new List<ICommand>
            {
                new AddCommand (),
                new RemoveCommand (),
                new ActiveCommand (),
                new AvailableCommand (),
                new AllCommand (),
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
                RequiredPermissions.Add(Discord.GuildPermission.ManageGuild);

                Aliases = new [] { "add" };
            }

            [Overload(typeof(void), "Add a new plugin from the list of available plugins.")]
            public Task<Result> Execute(CommandMetadata metadata, string pluginName)
            {
                ParentPlugin.AddPlugin(pluginName);
                if (ParentPlugin.GuildHandler.Plugins.IsPluginActive (pluginName))
                {
                    return TaskResult(null, $"Succesfully enabled plugin '{Plugin.GetFullName(PluginLoader.GetPlugin(pluginName))}' in this server.");
                }
                else
                {
                    IEnumerable<string> exceptions = ParentPlugin.GuildHandler.Plugins.GetInitializationExceptions().Select(x => x.Message);
                    return TaskResult(null, $"Failed to add plugin '{pluginName}'. Issues occured during initialization:\n\t{string.Join ("\n\t", exceptions)}");
                }
            }
        }

        private class RemoveCommand : PluginCommand<PluginManagerPlugin>
        {
            public RemoveCommand()
            {
                Name = "disable";
                Description = "Remove a plugin.";
                Category = AdditionalCategories.Management;
                RequiredPermissions.Add(Discord.GuildPermission.ManageGuild);
            
                Aliases = new [] { "remove" };
            }

            [Overload(typeof(void), "Remove a plugin from currently active plugins.")]
            public Task<Result> Execute(CommandMetadata metadata, string pluginName)
            {
                ParentPlugin.RemovePlugin(pluginName);
                return TaskResult(null, $"Sucessfully disabled plugin '{Plugin.GetFullName(PluginLoader.GetPlugin(pluginName))}' in this server.");
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
                return TaskResult(GetModuleListEmbed(ParentPlugin.GetAvailablePlugins().Where (x => !ParentPlugin.GetActivePlugins ().Any (y => y.GetType () == x)), "All available plugins.", "A list of all currently available, but not enabled plugins on this server."), null);
            }
        }

        private class AllCommand : PluginCommand<PluginManagerPlugin>
        {
            public AllCommand()
            {
                Name = "all";
                Description = "Every single plugins.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(Embed), "Display all plugins.")]
            public Task<Result> Execute(CommandMetadata metadata)
            {
                return TaskResult(GetModuleListEmbed(ParentPlugin.GetAvailablePlugins(), "All available plugins.", "A list of all currently available."), null);
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
                    string content = string.Join(", ", dependancies);
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
