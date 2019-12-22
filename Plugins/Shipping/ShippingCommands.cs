using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.Bot.Messaging.Advanced;
using Lomztein.Moduthulhu.Plugins.Standard;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.Shipping {
    public class ShippingCommands : PluginCommandSet<ShippingPlugin> {

        public ShippingCommands() {
            Name = "shipping";
            Description = "Most important commands.";
            Category = StandardCategories.Fun;

            _commandsInSet = new List<ICommand> {
                new Ship (), new Sink (),
                new Shipname (), new List (),
                new OTPs (), new ATPs (),
            };

            _defaultCommand = new Ship();
        }

        public class Ship : PluginCommand<ShippingPlugin> {

            public Ship() {
                Name = "ship";
                Description = "Ship two people.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (bool), "Ship two people so that they'll be together forever, at least in your headcanon.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
                var succesful = ParentPlugin.Ship (data.AuthorID, shippieOne.Id, shippieTwo.Id);
                if (succesful) {
                    return TaskResult (succesful, "Succesfully shipped " + shippieOne.GetShownName () + " and " + shippieTwo.GetShownName () + ", now known as " + ParentPlugin.GetShipName (ParentPlugin.GetShipByShippies (shippieOne.Id, shippieTwo.Id)) + ".");
                }
                return TaskResult (succesful, $"Failed to ship {shippieOne.GetShownName ()} x {shippieTwo.GetShownName ()} - You've already shipped them.");
            }

            [Overload (typeof (Ship), "Ship two people so that they'll be together forever, with a given custom name.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, string name) {
                ParentPlugin.NameShip (shippieOne.Id, shippieTwo.Id, name);
                return Execute (data, shippieOne, shippieTwo);
            }

        }

        public class Sink : PluginCommand<ShippingPlugin> {

            public Sink() {
                Name = "sink";
                Description = "Sink one of your ships.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (bool), "Sink one of your ships, in case the imaginative spark is gone.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
                var succesful = ParentPlugin.Sink (data.AuthorID, shippieOne.Id, shippieTwo.Id);
                if (succesful) {
                    return TaskResult (true, "Succesfully sunk " + ParentPlugin.GetShipName (ParentPlugin.GetShipByShippies (shippieOne.Id, shippieTwo.Id)) + ", at least for you.");
                }
                return TaskResult (false, $"Failed to sink that ship, as you have not shipped them yet.");
            }
        }

        public class Shipname : PluginCommand<ShippingPlugin> {

            public Shipname() {
                Name = "name";
                Description = "Specify ship name.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (void), "Name a ship something better than the shitty automatic name generator did.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, string name) {
                ParentPlugin.NameShip (shippieOne.Id, shippieTwo.Id, name);
                return TaskResult (null, $"{shippieOne.GetShownName ()} x {shippieTwo.GetShownName ()} has now been named {name}");
            }

            [Overload (typeof (void), "Reset a ship name back to the automatically generated one.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
                ParentPlugin.DeleteShipName (shippieOne.Id, shippieTwo.Id);
                Shipping.Ship ship = ParentPlugin.GetShipByShippies (shippieOne.Id, shippieTwo.Id);
                return TaskResult (null, $"{shippieOne.GetShownName ()} x {shippieTwo.GetShownName ()} has been named back to {ParentPlugin.GetShipName (ship)}.");
            }

        }

        public class List : PluginCommand<ShippingPlugin> {

            public List() {
                Name = "list";
                Description = "List all with someone.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (Embed), "List every single ship that you yourself is a part of.")]
            public Task<Result> Execute(CommandMetadata data) {
                return Execute (data, data.Message.Author as SocketGuildUser);
            }

            [Overload (typeof (Embed), "List every single ship that the given person is a part of.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                var ships = ParentPlugin.GetShippieShips (user.Id);
                var catagorized = ships.GroupBy (x => x.GetCompanionTo (user.Id));

                EmbedBuilder builder = new EmbedBuilder ();
                builder.WithAuthor (user).
                    WithTitle ("Ships containing " + user.GetShownName () + ".");

                List<EmbedFieldBuilder> embedFields = new List<EmbedFieldBuilder>();
                foreach (var ship in catagorized) {
                    var shippers = ship.Select (x => ParentPlugin.GuildHandler.GetUser (x.Shipper));

                    SocketGuildUser companion = ParentPlugin.GuildHandler.GetUser (ship.Key);
                    embedFields.Add (new EmbedFieldBuilder ().WithName ("Shipped with " + companion.GetShownName () + " as " + ParentPlugin.GetShipName (ship.FirstOrDefault()) + " " + ship.Count () + " times by:").WithValue (string.Join (", ", shippers)));
                }

                LargeEmbed largeEmbed = new LargeEmbed (builder, embedFields);
                return TaskResult(largeEmbed, null);
            }

        }

        public class OTPs : PluginCommand<ShippingPlugin> {

            public OTPs() {
                Name = "otps";
                Description = "Show your true pairs.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (string), "Show every single ship that you yourself has shipped.")]
            public Task<Result> Execute(CommandMetadata data) => Execute (data, data.Message.Author as SocketGuildUser);

            [Overload (typeof (string), "Show every single ship the given person has shipped.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                var allShips = ParentPlugin.GetAllShipperShips (user);
                StringBuilder result = new StringBuilder ($"{user.GetShownName ()} has shipped the following ships:```\n");
                foreach (Shipping.Ship ship in allShips) {
                    result.Append (ParentPlugin.ShipToString (ship) + "\n");
                }
                result.Append ("```");
                return TaskResult (result, result.ToString ());
            }
        }

        public class ATPs : PluginCommand<ShippingPlugin> {

            public ATPs() {
                Name = "atps";
                Description = "Show a leaderboard.";
                Category = StandardCategories.Fun;
            }

            [Overload (typeof (Embed), "Show a leaderboard of most shipped ships, sorted by most shipped.")]
            public Task<Result> Execute(CommandMetadata data) {
                var leaderboard = ParentPlugin.GetShipLeaderboard (data.Message.GetGuild ());

                EmbedBuilder builder = new EmbedBuilder ().
                    WithAuthor (ParentPlugin.GuildHandler.BotUser).
                    WithTitle ("All ships on " + data.Message.GetGuild ().Name + ".");

                List<EmbedFieldBuilder> embedFields = new List<EmbedFieldBuilder>();
                foreach (var pair in leaderboard)
                {
                    SocketGuildUser shippie = ParentPlugin.GuildHandler.GetUser(pair.Key);
                    var ships = string.Join("\n", ParentPlugin.GetShippieShips(shippie.Id).Select(x => ParentPlugin.ShipToString(x)));
                    embedFields.Add (new EmbedFieldBuilder ().WithName($"{shippie.GetShownName ()} has been shipped {pair.Value.Count} times in the following ships:\n").WithValue (ships));
                }

                LargeEmbed largeEmbed = new LargeEmbed (builder, embedFields);
                return TaskResult(largeEmbed, null);
            }
        }

    }
}