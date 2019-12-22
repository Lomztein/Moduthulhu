using Discord;
using Discord.Net;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Client;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    public class CoreAdminCommands : PluginCommandSet<AdministrationPlugin> {

        public CoreAdminCommands () {
            Name = "core";
            Description = "Core administration.";
            Category = AdditionalCategories.Management;

            _commandsInSet = new List<ICommand> {
                new ShutdownCommand (),
                new StatusCommand (),
                new SetAvatarCommand (),
                new SetUsernameCommand (),
                new VersionCommand (),
                new CreditsCommand (),
            };
        }


        private class ShutdownCommand : AdministratorCommand {

            public ShutdownCommand () {
                Name = "shutdown";
                Description = "Shutdown everything.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (void), "Shutdown the entire process and all of its clients. Requires manual rebooting.")]
            public Task<Result> Execute (CommandMetadata metadata) {
                Environment.Exit (0);
                return TaskResult (null, "Shutting down...");
            }

        }

        private class StatusCommand : PluginCommand<AdministrationPlugin>
        {
            public StatusCommand () {
                Name = "status";
                Description = "Check core status.";
                Category = AdditionalCategories.Management;
                Shortcut = "status";
            }

            [Overload (typeof (LargeEmbed), "Check the status for the core and its clients.")]
            public Task<Result> Execute (CommandMetadata _) {

                int shardIndex = 0;
                // TODO: Add status for shards and GuildHandlers.
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Core Process Status")
                    .WithAuthor(ParentPlugin.GuildHandler.BotUser)
                    .WithDescription(ParentPlugin.GuildHandler.Core.ToString())
                    .WithCurrentTimestamp();

                LargeEmbed embed = new LargeEmbed(builder, ParentPlugin.GuildHandler.Client.GetShardsStatus().Select(x => new EmbedFieldBuilder().WithName($"Shard {shardIndex++}").WithValue($"```{x}```")));
                return TaskResult (embed, "");
            }

            [Overload (typeof (LargeEmbed), "Check status for a specific shard.")]
            public Task<Result> Execute (CommandMetadata _, int shardId)
            {
                BotShard shard = ParentPlugin.GuildHandler.Shard.BotClient.GetShards().FirstOrDefault(x => x.ShardId == shardId);
                GuildHandler[] handlers = shard?.GetGuildHandlers();
                if (shard == null)
                {
                    return TaskResult(null, $"Failed to fetch status for shard {shardId}, it may be running on a different physical process. ShardsIds on this process range from {ParentPlugin.GuildHandler.Client.Configuration.ShardRange.Min} to {ParentPlugin.GuildHandler.Client.Configuration.ShardRange.Max - 1}");
                }
                int fieldIndex = 1;
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"Shard {shardId} status")
                    .WithDescription($"```{shard}```");

                return TaskResult(new LargeEmbed (builder, string.Join("\n", handlers.Select(x => x.ToString())).SplitMessage("```", 1000).Select(x => new EmbedFieldBuilder().WithName($"Field {fieldIndex++}").WithValue(x))), string.Empty);
            }

            [Overload (typeof (Embed), "Check status of a specific GuildHandler")]
            public Task<Result> Execute (CommandMetadata _, string guildHandler)
            {
                GuildHandler handler = ParentPlugin.GuildHandler.Shard.GetGuildHandlers().FirstOrDefault(x => x.Name.ToUpperInvariant() == guildHandler.ToUpperInvariant());
                if (handler != null)
                {
                    string status = handler.ToString();
                    string activePlugins = string.Join("\n", handler.Plugins.GetActivePlugins().Select(x => Plugin.GetVersionedFullName(x.GetType())));

                    return TaskResult(new EmbedBuilder()
                        .WithTitle(handler.Name + " status")
                        .WithDescription($"```{status}```")
                        .AddField("Active Plugins", $"```{activePlugins}```")
                        .WithCurrentTimestamp()
                        .Build(), string.Empty);
                }
                else
                {
                    return TaskResult(null, $"Failed to fetch GuildHandler status, no GuildHandler matching '{guildHandler}' could be found.");
                }

            }
        }

        private class SetUsernameCommand : AdministratorCommand
        {
            public SetUsernameCommand()
            {
                Name = "setusername";
                Description = "Set client username.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(void), "Set the clients username to something new.")]
            public async Task<Result> Execute(CommandMetadata metadata, string newUsername)
            {
                try
                {
                    await ParentPlugin.GuildHandler.BotUser.ModifyAsync(x => x.Username = newUsername);
                }
                catch (RateLimitedException)
                {
                    throw new InvalidExecutionException("Rate limit exceeded, please wait a while before trying again.");
                }
                catch (HttpException)
                {
                    throw new InvalidExecutionException("Username might be too long or contain invalid characters.");
                }
                return new Result(null, $"Changed client username to **{newUsername}**.");
            }
        }

        private class SetAvatarCommand : AdministratorCommand
        {
            public SetAvatarCommand()
            {
                Name = "setavatar";
                Description = "Set client avatar.";
                Category = AdditionalCategories.Management;
            }

            [Overload(typeof(void), "Set the clients avatar to something from a website.")]
            public async Task<Result> Execute(CommandMetadata metadata, string uri)
            {
                Uri address = new Uri(uri);
                using (WebClient client = new WebClient())
                using (Stream stream = await client.OpenReadTaskAsync(address))
                {
                    Discord.Image image = new Discord.Image(stream);
                    try
                    {
                        await ParentPlugin.GuildHandler.BotUser.ModifyAsync(x => x.Avatar = image);
                    }
                    catch (RateLimitedException)
                    {
                        throw new InvalidExecutionException("Rate limit exceeded, please wait a while before trying again.");
                    }
                    catch (HttpException)
                    {
                        throw new InvalidExecutionException("Image was invalid.");
                    }
                }
                return new Result(null, "Succesfully changed avatar to the one found at " + uri);
            }
        }

        private class VersionCommand : Command
        {
            public VersionCommand()
            {
                Name = "version";
                Description = "Show core version.";
                Category = AdditionalCategories.Management;
                Aliases = new [] { "ver" };
                Shortcut = "version";
            }

            [Overload (typeof (string), "Return the current version of the bot core framework.")]
            public Task<Result> Execute (CommandMetadata metadata)
            {
                string version = Assembly.GetEntryAssembly().GetName().Version.ToString ();
                return TaskResult(version, $"Current Moduthulhu core version is '{version}'");
            }
        }

        private class CreditsCommand : PluginCommand<AdministrationPlugin>
        {
            public CreditsCommand ()
            {
                Name = "credits";
                Description = "Display bot credits.";
                Category = AdditionalCategories.Management;
                Shortcut = "credits";
            }

            [Overload (typeof (string), "Credits those amazing beautiful handsome sexy people that created this bot core.")]
            public Task<Result> Execute (CommandMetadata _)
            {
                StringBuilder credits = new StringBuilder();
                credits.AppendLine("Bot core is created by Marcus \"Lomztein\" Jensen *(https://github.com/Lomztein)*, as a hobby passion project and passingly sentient slave.\n");
                credits.AppendLine("Additional council and help by the glorious Frederik \"Fred\" Rosenberg *(https://github.com/Frede175)*, the outrageously attractive Younes \"drcd\" Zakaria *(https://github.com/drcd)*, the suave servermaster Thorvald \"Purvaldur\" Kjartansson *(https://github.com/purvaldur)*, and the bona fide baguette Mph!");
                credits.AppendLine("Patience for listening to incomprehensible, overly excited ~~and mildly aroused~~ explanations of inner workings provided by the illustrious Victor \"Nyx\" Koch!\n");
                credits.AppendLine("Suffering and despair though testing the many awful versions Adminthulhu/Moduthulhu by my magnificant friends of Monster Mash!\n");
                credits.AppendLine("Thanks to the following for extending Moduthulhu by creating plugins: " + string.Join(", ", ParentPlugin.GuildHandler.Plugins.GetActivePlugins().Select(x => Plugin.GetAuthor(x.GetType())).Distinct()) + "!\n");
                credits.AppendLine("And thanks to you, for ~~unknowingly sacrifising yourself to my ever growing hunger~~ making use of my services :)");
                return TaskResult(credits.ToString(), credits.ToString());
            }
        }

    }
}
