using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
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

            _commandsInSet = new List<ICommand>
            {
                new AddCommand (),
                new RemoveCommand (),
                new ActiveCommand (),
                new AvailableCommand (),
                new AllCommand (),
                new InfoCommand (),
            };

            _defaultCommand = new AvailableCommand();
        }

        private class AddCommand : PluginCommand<PluginManagerPlugin>
        {
            public AddCommand ()
            {
                Name = "enable";
                Description = "Add a plugin.";
                Category = AdditionalCategories.Management;
                RequiredPermissions.Add(GuildPermission.ManageGuild);
                Shortcut = "enableplugin";

                Aliases = new [] { "add" };
                ShortcutAliases = new[] { "addplugin" };
            }

            [Overload(typeof(Embed), "Add a new plugin from the list of available plugins.")]
            public async Task<Result> Execute(ICommandMetadata metadata, string pluginName)
            {
                Type plugin = PluginLoader.GetPlugin(pluginName);
                if (plugin != null)
                {
                    string name = Plugin.GetName(plugin);
                    if (!ParentPlugin.GuildHandler.Plugins.IsPluginActive(pluginName))
                    {
                        QuestionMessage question = new QuestionMessage($"Are you sure you wish to enable plugin '{name}'?", async () =>
                        {
                            ParentPlugin.AddPlugin(pluginName);
                            if (ParentPlugin.GuildHandler.Plugins.IsPluginActive(pluginName))
                            {
                                var state = ParentPlugin.GuildHandler.State;
                                await metadata.Channel.SendMessageAsync(string.Empty, false, state.ChangesToEmbed ($"Succesfully enabled plugin '{name}' on this server."));
                            }
                            else
                            {
                                IEnumerable<string> exceptions = ParentPlugin.GuildHandler.Plugins.GetInitializationExceptions().Select(x => x.InnerException.Message);
                                await metadata.Channel.SendMessageAsync($"Failed to add plugin '{name}'. Issues occured during initialization:\n\t{string.Join("\n\t", exceptions)}");
                            }
                        }, async () => await metadata.Channel.SendMessageAsync($"Cancelled enabled plugin '{name}'.")).SetRecipient (metadata.AuthorId);
                        await metadata.Channel.SendMessageAsync(null, false, GetModuleEmbed(PluginLoader.GetPlugin(pluginName)));
                        return new Result (question, string.Empty);
                    }
                    else
                    {
                        return new Result(null, $"Failed to enable plugin: '{name}' is already enabled.");
                    }
                }
                else
                {
                    return new Result(null, $"Failed to enable plugin: No available plugins matches '{pluginName}' Check `!plugin available` to get a list of available plugins.");
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
                RequiredPermissions.Add(GuildPermission.ManageGuild);
                Shortcut = "disableplugin";
            
                Aliases = new [] { "remove" };
                ShortcutAliases = new[] { "removeplugin" };
            }

            [Overload(typeof(Embed), "Remove a plugin from currently active plugins.")]
            public Task<Result> Execute(ICommandMetadata _, string pluginName)
            {
                ParentPlugin.RemovePlugin(pluginName);
                var state = ParentPlugin.GuildHandler.State;
                return TaskResult(state.ChangesToEmbed($"Successfully disabled plugin '{Plugin.GetName(PluginLoader.GetPlugin(pluginName))}' on this server."), string.Empty);
            }
        }

        private class ActiveCommand : PluginCommand<PluginManagerPlugin>
        {
            public ActiveCommand()
            {
                Name = "active";
                Description = "Active plugins.";
                Category = AdditionalCategories.Management;
                Aliases = new[] { "enabled", "running" };   
            }

            [Overload(typeof(Embed), "Display all currently active plugins on this server.")]
            public Task<Result> Execute(ICommandMetadata metadata)
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
            public Task<Result> Execute(ICommandMetadata metadata)
            {
                return TaskResult(GetModuleListEmbed(PluginManagerPlugin.GetAvailablePlugins().Where (x => !ParentPlugin.GetActivePlugins ().Any (y => y.GetType () == x)), "All available plugins.", "A list of all currently available, but not enabled plugins on this server."), null);
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
            public Task<Result> Execute(ICommandMetadata metadata)
            {
                return TaskResult(GetModuleListEmbed(PluginManagerPlugin.GetAvailablePlugins(), "All available plugins.", "A list of all currently available."), null);
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
            public Task<Result> Execute(ICommandMetadata metadata, string pluginName)
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

            string[] dependencies = PluginLoader.DependencyTree.GetDependencies(Plugin.GetVersionedFullName(moduleType)).Select(x => Plugin.GetName(x) + " by " + Plugin.GetAuthor (x) + ", version " + Plugin.GetVersion (x)).ToArray();
            if (dependencies.Length > 0)
            {
                string content = $"```{string.Join('\n', dependencies)}```";
                builder.AddField("Dependencies", content);
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

        public static LargeEmbed GetModuleListEmbed(IEnumerable<Type> pluginTypes, string title, string description)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor("Plugin Information")
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(pluginTypes.Count () + " plugins.");

            return new LargeEmbed (builder, pluginTypes.Select (x => new EmbedFieldBuilder ().WithName (Plugin.GetName(x)).WithValue ($"> {Plugin.GetDescription(x)}\n*Created by {Plugin.GetAuthor(x)}*.\n\u200B")));
        }
    }
}
