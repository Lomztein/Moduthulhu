using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Plugins.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.GameServerMonitor
{
    public class ServerMonitorCommandSet : PluginCommandSet<GameServerMonitorPlugin>
    {
        public ServerMonitorCommandSet()
        {
            Name = "gsm";
            Description = "Game server monitor";
            Category = StandardCategories.Utility;
            RequiredPermissions.Add(Discord.GuildPermission.ManageGuild);

            _commandsInSet = new List<ICommand>
            {
                new ListServerMonitors(),
                new AddServerMonitor(),
                new RemoveServerMonitor(),
                new ForcePoll()
            };

            _defaultCommand = _commandsInSet[0];
        }

        public class ListServerMonitors : PluginCommand<GameServerMonitorPlugin>
        {
            public ListServerMonitors()
            {
                Name = "list";
                Description = "List current server monitors";
                Category = StandardCategories.Utility;
            }

            [Overload(typeof(void), "List all current server monitors.")]
            public Task<Result> Execute(CommandMetadata metadata)
            {
                var current = ParentPlugin.ServersToMonitor.GetValue();
                if (current.Count > 0)
                {
                    EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle("Current server monitors")
                        .WithDescription($"```\n{string.Join("\n", current.Select(x => x.ServerName + " at '" + x.HostName + "'"))}```");
                    return TaskResult(builder.Build(), string.Empty);
                }
                else
                {
                    return TaskResult(null, "There are currently no servers being monitored.");
                }
            }
        }

        public class AddServerMonitor : PluginCommand<GameServerMonitorPlugin>
        {
            public AddServerMonitor ()
            {
                Name = "add";
                Description = "Add server monitor";
                Category = StandardCategories.Utility;
                RequiredPermissions.Add(GuildPermission.ManageGuild);
            }

            [Overload(typeof (bool), "Add a new server monitor. Returns true if succesful, otherwise false.")]
            public async Task<Result> Execute (CommandMetadata metadata, string gameName, string serverName, string hostName, string type)
            {
                type = type.ToLowerInvariant();
                if (type != "embed" && type != "text")
                {
                    return new Result(false, "Type must be 'embed' or 'text'.");
                }

                bool useEmbed = type == "embed";
                if (ParentPlugin.ServerMonitorExists(serverName))
                {
                    return new Result(false, $"Failed to add new server monitor: Monitor for server '{serverName}' already exists.");
                }
                else
                {
                    if (ParentPlugin.HasMonitor(gameName))
                    {
                        var message = await ParentPlugin.CreateServerMonitoringMessage(metadata.Message.Channel as SocketTextChannel);
                        var info = new GameServerMonitorInfo(gameName, serverName, hostName, message.Channel.Id, message.Id, useEmbed);
                        ParentPlugin.AddServerToMonitor(info);
                        await ParentPlugin.PollServers();
                        return new Result(true, string.Empty);
                    }
                    else
                    {
                        return new Result(false, $"Failed to add new server monitor: Monitoring for game '{gameName}' not supported.");
                    }
                }
            }
        }

        public class RemoveServerMonitor : PluginCommand<GameServerMonitorPlugin>
        {
            public RemoveServerMonitor()
            {
                Name = "remove";
                Description = "Remove server monitor";
                Category = StandardCategories.Utility;
                RequiredPermissions.Add(GuildPermission.ManageGuild);
            }

            [Overload(typeof(bool), "Remove a server monitor. Returns true if succesful, otherwise false.")]
            public async Task<Result> Execute(CommandMetadata metadata, string serverName)
            {
                var info = ParentPlugin.GetServerMonitor(serverName);
                if (ParentPlugin.RemoveServerToMonitor(serverName))
                {
                    await ParentPlugin.DeleteServerMonitoringMessage(info);
                    return new Result(true, $"Succesfully removed server monitor '{serverName} for '{info.GameName}' at '{info.HostName}'.");
                }
                else
                {
                    return new Result(true, $"Failed to remove server monitor '{serverName}: Monitor not found.");
                }
            }
        }

        public class ForcePoll : PluginCommand<GameServerMonitorPlugin>
        {
            public ForcePoll()
            {
                Name = "poll";
                Description = "Poll server monitors";
                Category = StandardCategories.Utility;
                RequiredPermissions.Add(GuildPermission.ManageGuild);
            }

            [Overload(typeof(bool), "Force a poll of all server monitors.")]
            public async Task<Result> Execute(CommandMetadata metadata)
            {
                await ParentPlugin.PollServers();
                return new Result(null, "Succesfully polled servers.");
            }

            [Overload(typeof(bool), "Force a poll of a particular server monitor.")]
            public async Task<Result> Execute(CommandMetadata metadata, string serverName)
            {
                var info = ParentPlugin.GetServerMonitor(serverName);
                if (info == null)
                {
                    return new Result(null, $"Failed to poll server: Monitor for '{serverName}' not found.");
                }
                else
                {
                    await ParentPlugin.PollServer(info);
                    return new Result(null, $"Succesfully polled server '{serverName}'");
                }
            }
        }
    }
}
