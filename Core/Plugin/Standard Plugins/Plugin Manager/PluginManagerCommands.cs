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
                if (pluginType != null)
                {
                    return TaskResult(GetModuleEmbed(pluginType), null);
                }
                else
                {
                    return TaskResult(null, $"Failed to find any plugins matching '{pluginName}'");
                }
            }
        }

        public static Embed GetModuleEmbed(Type moduleType)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(Plugin.GetName(moduleType))
                .WithDescription(Plugin.GetDescription(moduleType))
                .WithAuthor("Plugin Information")
                .WithFooter("Created by " + Plugin.GetAuthor(moduleType));

            if (Plugin.IsCritical (moduleType))
            {
                builder.AddField("Notice: ", "This plugin is critical and cannot be disabled.", true);
            }

            string[] dependancies = PluginLoader.DependancyTree.GetDependencies(Plugin.GetVersionedFullName(moduleType)).Select(x => Plugin.GetVersionedFullName(x)).ToArray();
            if (dependancies.Length > 0)
            {
                string content = $"```{string.Join('\n', dependancies)}```";
                builder.AddField("Dependancies", content);
            }

            if (Plugin.GetAuthorURI (moduleType) != null)
            {
                string authorUrl = $"Author URL: {(Plugin.GetAuthorURI(moduleType) == null ? "Unspecified" : Plugin.GetAuthorURI(moduleType).AbsoluteUri)}";
                string pluginUrl = $"Plugin URL: {(Plugin.GetProjectURI(moduleType) == null ? "Unspecified" : Plugin.GetProjectURI(moduleType).AbsoluteUri)}";
                string patchUrl = $"Patch URL: {(Plugin.GetPatchURI(moduleType) == null ? "Unspecified" : Plugin.GetPatchURI(moduleType).AbsoluteUri)}";

                builder.AddField("Plugin Source", $"{authorUrl}\n{pluginUrl}\n{patchUrl}");
            }

            GDPRCompliance? compliance = Plugin.GetGDPRCompliance(moduleType);
            string header = null;
            if (compliance.HasValue)
            {
                switch (compliance.Value)
                {
                    case GDPRCompliance.Full:
                        header = "This plugin is fully GDPR compliant.";
                        break;

                    case GDPRCompliance.Partial:
                        header = "This plugin is only partially GDPR compliant.";
                        break;

                    case GDPRCompliance.None:
                        header = "WARNING: This plugin does not comply with GDPR.";
                        break;
                }

                string notes = Plugin.GetGDPRNotes(moduleType).Length == 0 ? "```No notes on compliance.```" : $"```{string.Join('\n', Plugin.GetGDPRNotes(moduleType))}```";
                builder.AddField(header, notes);
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
