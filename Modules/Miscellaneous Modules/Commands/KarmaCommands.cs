using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Modules.Misc.Karma;
using Lomztein.Moduthulhu.Modules.Misc.Karma.Extensions;
using Discord;
using Lomztein.AdvDiscordCommands.Framework.Categories;

namespace Lomztein.Moduthulhu.Modules.Misc.Karma.Commands
{
    public class KarmaCommand : ModuleCommand<KarmaModule> {

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
            KarmaModule.Selfworth karma = ParentModule.GetKarma (user.Id);
            return TaskResult (karma.Total, $"User {user.GetShownName ()} has {karma.Total} karma! (+{karma.upvotes} / -{karma.downvotes})");
        }

        [Overload (typeof (SocketGuildUser[]), "Returns karma of a given user.")]
        public Task<Result> Execute (CommandMetadata data, int amount) {
            var allKarma = ParentModule.GetKarmaDictionary ();
            List<SocketGuildUser> inGuild = new List<SocketGuildUser> ();

            foreach (var entry in allKarma) { // Man I'm getting lazy with dictionary type naming. All those generic parameters yo.
                SocketGuildUser user = data.Message.GetGuild ()?.GetUser (entry.Key);
                if (user == null)
                    continue;
                inGuild.Add (user);
            }

            inGuild.Sort (new KarmaComparator (ParentModule));
            inGuild = inGuild.GetRange (0, Math.Min (amount, inGuild.Count));

            string result = "```";
            foreach (SocketGuildUser user in inGuild) {
                result += StringExtensions.UniformStrings (user.GetShownName (), ParentModule.GetKarma (user.Id).ToString ()) + "\n";
            }
            result += "```";

            return TaskResult (inGuild, result);
        }
    }
}
