using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Misc.Color
{
    public class ColourModule : ModuleBase, IConfigurable<MultiConfig> {

        public const string PREFIX = "cl_";

        public override string Name => "COLOURS";
        public override string Description => "This module is scientifically proven to improve funkyness by about 4%.";
        public override string Author => "Lomztein";

        public override string [ ] RequiredModules => new string [ ] { "Lomztein_Command Root" };

        public override bool Multiserver => true;

        public MultiConfig Configuration { get; set; } = new MultiConfig ();

        private MultiEntry<bool> autocolourNewJoins;
        protected MultiEntry<Dictionary<ulong, string>> colourIdentification;

        private SetColour colorCommand = new SetColour ();

        public void Configure() {
            List<SocketGuild> guilds = ParentBotClient.discordClient.Guilds.ToList ();
            colourIdentification = Configuration.GetEntries (guilds, "Colours", guilds.Select (x => x.Roles.Where (y => y.Name.StartsWith (PREFIX)).ToDictionary (z => z.Id, z => z.Name.Substring (PREFIX.Length))));
            autocolourNewJoins = Configuration.GetEntries (guilds, "AutoColour", true);
        }

        public override void Initialize() {
            colorCommand.parentModule = this;
            ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot.AddCommands (colorCommand);
            ParentBotClient.discordClient.UserJoined += OnUserJoined;
        }

        private async Task OnUserJoined(SocketGuildUser guildUser) {
            Dictionary<ulong, string> localColours = colourIdentification.GetEntry (guildUser.Guild);
            SocketRole randomRole = ParentBotClient.GetRole (guildUser.Guild.Id, localColours.ElementAt (new Random ().Next (localColours.Count)).Key);
            await guildUser.AsyncSecureAddRole (randomRole);
        }

        public override void Shutdown() {
            ParentModuleHandler.GetModule<CommandRootModule> ().commandRoot.RemoveCommands (colorCommand);
            ParentBotClient.discordClient.UserJoined -= OnUserJoined;
        }

        public class SetColour : ModuleCommand<ColourModule> {

            public SetColour () {
                command = "setcolour";
                shortHelp = "Set your personal color to something funky.";
                catagory = Category.Utility;
            }

            [Overload (typeof (void), "Set your colour to something cool!")]
            public async Task<Result> Execute(CommandMetadata data, string colorName) {

                if (data.message.Author is SocketGuildUser guildUser) {

                    IEnumerable<SocketRole> currentRoles = guildUser.Roles.Where (x => parentModule.colourIdentification.GetEntry (guildUser.Guild).ContainsKey (x.Id));

                    SocketRole role = null;
                    string name = "";

                    foreach (var entry in parentModule.colourIdentification.GetEntry (data.message.GetGuild ())) {
                        if (entry.Value.ToUpper () == colorName.ToUpper ()) {
                            role = parentModule.ParentBotClient.GetRole (data.message.GetGuild ().Id, entry.Key);
                            name = entry.Value;
                            break;
                        }
                    }

                    await guildUser.RemoveRolesAsync (currentRoles);
                    if (role != null)
                        await guildUser.AsyncSecureAddRole (role);

                    return new Result (null, role == null ? "Failed to colour you, colour not found." : $"You've been succesfully coloured **{name}**!");
                }

                return new Result (null, null);
            }
        }
    }
}
