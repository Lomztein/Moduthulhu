using Discord;
using Discord.Net;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Client;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Plugins.Standard
{
    public class CoreAdminCommands : PluginCommandSet<AdministrationPlugin> {

        public CoreAdminCommands () {
            Name = "core";
            Description = "Core administration.";
            Category = AdditionalCategories.Management;

            commandsInSet = new List<ICommand> {
                new ShutdownCommand (),
                new StatusCommand (),
                new SetAvatarCommand (),
                new SetUsernameCommand (),
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

        private class StatusCommand : AdministratorCommand
        {

            public StatusCommand () {
                Name = "status";
                Description = "Check core status.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (LargeEmbed), "Check the status for the core and its clients.")]
            public Task<Result> Execute (CommandMetadata metadata) {
                LargeEmbed embed = new LargeEmbed();

                // TODO: Add status for shards and GuildHandlers.
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Core Process Status")
                    .WithAuthor(ParentPlugin.GuildHandler.BotUser)
                    .WithDescription(ParentPlugin.GuildHandler.Core.GetStatusString())
                    .WithCurrentTimestamp();

                embed.CreateFrom(builder);
                return TaskResult (embed, "");
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
    }
}
