using Discord.Net;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.CustomCommands.Categories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Administration.AdministrationCommands
{
    public class ClientAdminCommands : ModuleCommandSet<AdministrationModule>
    {
        public class ClientAdminCommand : AdministratorCommand {

            public ClientAdminCommand() {
                AdministratorSource = (() => ParentModule.ParentShard.BotClient.ClientAdministrators);
                AdministratorTypeName = "client";
            }

        }

        public ClientAdminCommands () {
            Name = "client";
            Description = "Client administration.";
            Category = AdditionalCategories.Management;

            commandsInSet = new List<ICommand> {
                new RestartCommand (),
                new SetUsernameCommand (),
                new SetAvatarCommand (),
            };
        }

        public class RestartCommand : ClientAdminCommand {

            public RestartCommand () : base () {
                Name = "restart";
                Description = "Restart client.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (void), "Restart the client if something is messed up. Might take a while if it's on a lot of servers.")]
            public async Task<Result> Execute (CommandMetadata metadata) {
                await ParentModule.ParentShard.BotClient.ClientManager.RestartClient (ParentModule.ParentShard.BotClient);
                return new Result (null, "Restarting client...");
            }

        }

        public class SetUsernameCommand : ClientAdminCommand {

            public SetUsernameCommand () : base () {
                Name = "setusername";
                Description = "Set client username.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (void), "Set the clients username to something new.")]
            public async Task<Result> Execute(CommandMetadata metadata, string newUsername) {
                try {
                    await ParentModule.ParentShard.BotClient.FirstClient.CurrentUser.ModifyAsync (x => x.Username = newUsername);
                } catch (RateLimitedException) {
                    throw new InvalidExecutionException ("Rate limit exceeded, please wait a while before trying again.");
                } catch (HttpException) {
                    throw new InvalidExecutionException ("Username might be too long or contain invalid characters.");
                }
                return new Result (null, $"Changed client username to **{newUsername}**.");
            }
        }

        public class SetAvatarCommand : ClientAdminCommand {

            public SetAvatarCommand() : base () {
                Name = "setavatar";
                Description = "Set client avatar.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (void), "Set the clients avatar to something from a website.")]
            public async Task<Result> Execute(CommandMetadata metadata, string uri) {
                Uri address = new Uri (uri);
                using (WebClient client = new WebClient ())
                using (Stream stream = await client.OpenReadTaskAsync (address)) {
                    Discord.Image image = new Discord.Image (stream);
                    try {
                        await ParentModule.ParentShard.BotClient.FirstClient.CurrentUser.ModifyAsync (x => x.Avatar = image);
                    } catch (RateLimitedException) {
                        throw new InvalidExecutionException ("Rate limit exceeded, please wait a while before trying again.");
                    } catch (HttpException) {
                        throw new InvalidExecutionException ("Image was invalid.");
                    }
                }
                return new Result (null, "Succesfully changed avatar to the one found at " + uri);
            }
        }
    }
}
