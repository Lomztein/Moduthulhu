using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Plugins.Standard;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Misc.Karma.Commands
{
    public class KarmaCommand : PluginCommand<KarmaPlugin> {

        public KarmaCommand() {
            Name = "karma";
            Description = "Shows karma.";
            Category = StandardCategories.Fun;
        }

        [Overload (typeof (int), "Returns your own karma.")]
        public Task<Result> Execute (CommandMetadata data) {
            return Execute (data, data.Message.Author);
        }

        [Overload (typeof (int), "Returns karma of a given user.")]
        public Task<Result> Execute (CommandMetadata data, IUser user) {
            KarmaPlugin.Selfworth karma = ParentPlugin.GetKarma (user.Id);
            return TaskResult (karma.Total, $"User {user.GetShownName ()} has {karma.Total} karma! (+{karma.Upvotes} / -{karma.Downvotes})");
        }

        [Overload (typeof (SocketGuildUser[]), "Returns karma of a given user.")]
        public Task<Result> Execute (CommandMetadata data, int amount) {
            var allKarma = ParentPlugin.GetKarmaDictionary ();
            List<SocketGuildUser> inGuild = new List<SocketGuildUser> ();

            foreach (var entry in allKarma) { // Man I'm getting lazy with dictionary type naming. All those generic parameters yo.
                SocketGuildUser user = data.Message.GetGuild ()?.GetUser (entry.Key);
                if (user == null)
                {
                    continue;
                }
                inGuild.Add (user);
            }

            var ordered = inGuild.OrderByDescending(x => ParentPlugin.GetKarma (x.Id).Total).ToList ();
            var inRange = ordered.GetRange (0, Math.Min (amount, inGuild.Count));

            StringBuilder result = new StringBuilder ("```");
            foreach (SocketGuildUser user in inRange) {
                result.Append (StringExtensions.UniformStrings (user.GetShownName (), ParentPlugin.GetKarma (user.Id).ToString ()) + "\n");
            }
            result.Append ("```");

            return TaskResult (inGuild, result.ToString ());
        }
    }
}
