using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Plugins.Standard;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;

namespace Lomztein.Moduthulhu.Modules.Colour
{
    [Descriptor ("Lomztein", "COLOURS!", "Plugin scientifically proven to increase funkyness by between negative thirty two, to about four percent.")]
    [Source ("https://github.com/Lomztein", "https://github.com/Lomztein/Moduthulhu/blob/master/Plugins/Miscellaneous/ColorPlugin.cs")]
    [Dependency ("Lomztein-Command Root")]
    public class ColourPlugin : PluginBase {

        private SetColour _command;

        private CachedValue<string> _colourRolePrefix;
        private CachedValue<bool> _colourOnJoin;

        public override void Initialize() {
            _command = new SetColour { ParentPlugin = this };

            GuildHandler.UserJoined += OnUserJoined;
            SendMessage("Lomztein-Command Root", "AddCommand", _command);

            _colourRolePrefix = GetConfigCache("ColourRolePrefix", x => "cl_");
            _colourOnJoin = GetConfigCache("ColourOnJoin", x => false);

            AddConfigInfo<string>("Set Colour Prefix", "Set colour prefix", x => _colourRolePrefix.SetValue(x), x => $"Set colour prefix to '{x}'", "Prefix");
            AddConfigInfo("Set Colour Prefix", "Get colour prefix", () => $"Current colour prefix is '{_colourRolePrefix.GetValue()}'.");

            AddConfigInfo("Colour on Join", "Colour on join?", () => _colourOnJoin.SetValue (!_colourOnJoin.GetValue ()), () => _colourOnJoin.GetValue() ? $"New members now be automatically given a random available colour on join." : $"New members will no longer be coloured on join.");
        }

        public override void Shutdown()
        {
            GuildHandler.UserJoined -= OnUserJoined;
            SendMessage("Lomztein-Command Root", "RemoveCommand", _command);
        }

        private async Task OnUserJoined(SocketGuildUser guildUser) {
            await GiveRandomColourAsync (guildUser);
        }

        private async Task GiveRandomColourAsync (SocketGuildUser guildUser) {
            SocketRole[] roles = GetRoles();
            SocketRole randomRole = roles[new Random ().Next (roles.Length)];
            await guildUser.AsyncSecureAddRole (randomRole);
        }

        private SocketRole[] GetRoles() => GuildHandler.GetGuild().Roles.Where(x => x.Name.StartsWith(_colourRolePrefix.GetValue())).ToArray();

        public class SetColour : PluginCommand<ColourPlugin> {

            public SetColour() {
                Name = "setcolour";
                Description = "Funkyfy yourself.";
                Category = StandardCategories.Utility;
            }

            [Overload(typeof(void), "Set your colour to something cool!")]
            public async Task<Result> Execute(CommandMetadata data, string colorName) {

                SocketGuildUser guildUser = data.Message.Author as SocketGuildUser;
                IEnumerable<SocketRole> currentRoles = guildUser.Roles.Where(x => ParentPlugin.GetRoles().Any(y => x.Id == y.Id));

                SocketRole role = null;

                foreach (var entry in ParentPlugin.GetRoles ()) {

                    string roleName = entry.Name.Substring(ParentPlugin._colourRolePrefix.GetValue().Length).ToUpperInvariant();
                    if (colorName.ToUpperInvariant () == roleName) {
                        role = entry;
                        break;
                    }
                }

                await guildUser.RemoveRolesAsync(currentRoles);
                if (role != null)
                {
                    await guildUser.AddRoleAsync(role);
                }

                return new Result(null, role == null ? "Failed to colour you, colour not found." : $"You've been succesfully coloured **{colorName}**!");
            }

            [Overload (typeof (SocketRole[]), "Return a list of all available colours.")]
            public Task<Result> Execute (CommandMetadata _)
            {
                string colours = string.Join(", ", ParentPlugin.GetRoles().Select(x => x.Name.Substring (ParentPlugin._colourRolePrefix.GetValue ().Length)));
                return TaskResult(ParentPlugin.GetRoles(), "These colours are available: " + colours);
            }
        }
    }
}
