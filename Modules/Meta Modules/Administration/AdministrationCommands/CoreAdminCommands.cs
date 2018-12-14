using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Client;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.CustomCommands.Categories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Administration.AdministrationCommands
{
    public class CoreAdminCommands : ModuleCommandSet<AdministrationModule> {

        public class CoreAdminCommand : AdministratorCommand {

            public CoreAdminCommand () {
                AdministratorSource = (() => ParentModule.ParentShard.Core.BotAdministrators);
                AdministratorTypeName = "core";
            }

        }


        public CoreAdminCommands () {
            Name = "core";
            Description = "Core administration.";
            Category = AdditionalCategories.Management;

            commandsInSet = new List<ICommand> {
                new ShutdownCommand (),
                new RestartCommand (),
                new StatusCommand (),
                new PatchCommand (),
            };
        }


        public class ShutdownCommand : CoreAdminCommand {

            public ShutdownCommand () : base () {
                Name = "shutdown";
                Description = "Shutdown everything.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (void), "Shutdown the entire process and all of its clients. Requires manual rebooting.")]
            public Task<Result> Execute (CommandMetadata metadata) {
                Cross.Status.Set ("IsRunning", false);
                Environment.Exit (0);
                return TaskResult (null, "Shutting down...");
            }

        }

        public class RestartCommand : CoreAdminCommand {

            public RestartCommand () : base () {
                Name = "restart";
                Description = "Restart everything.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (void), "Shutdowns and restarts the process, provided the process is running through the Upkeeper.")]
            public Task<Result> Execute (CommandMetadata metadata) {
                Environment.Exit (0);
                return TaskResult (null, "Restarting...");
            }

        }

        public class StatusCommand : CoreAdminCommand {

            public StatusCommand () : base () {
                Name = "status";
                Description = "Check core status.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (LargeEmbed), "Check the status for the core and its clients.")]
            public Task<Result> Execute (CommandMetadata metadata) {
                LargeEmbed embed = new LargeEmbed();

                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Core Process Status")
                    .WithAuthor(ParentModule.ParentShard.Client.CurrentUser)
                    .WithDescription(ParentModule.ParentShard.Core.GetStatusString())
                    .WithCurrentTimestamp();

                // Lasagna is one of my favorite foods.
                foreach (BotClient client in ParentModule.ParentShard.BotClient.ClientManager.ClientSlots)
                {
                    builder.AddField(client.GetStatusString(), "```" + client.GetShardsStatus() + "```");
                }

                embed.CreateFrom(builder);
                return TaskResult (embed, "");
            }

        }

        public class PatchCommand : CoreAdminCommand {

            public PatchCommand () : base () {
                Name = "patch";
                Description = "NOT IMPLEMENTED.";
                Category = AdditionalCategories.Management;
                CommandEnabled = false;
            }

            [Overload (typeof (void), "Check for updates and patch bot if any are available.")]
            public Task<Result> Execute (CommandMetadata metadata) {
                throw new NotImplementedException ("This command hasn't yet been implemented.");
            }

        }
    }
}
