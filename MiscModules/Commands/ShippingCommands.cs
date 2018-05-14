using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;

namespace Lomztein.Moduthulhu.Modules.Misc.Shipping.Commands
{
    public class ShippingCommands : ModuleCommandSet<ShippingModule> {

        public ShippingCommands () {
            command = "shipping";
            shortHelp = "The most important commands.";
            catagory = Category.Fun;

            commandsInSet = new List<Command> () {
                new Ship (), new Sink (),
                new Name (), new List (),
                new OTPs (), new ATPs (),
            };
        }

        public class Ship : ModuleCommand<ShippingModule> {

            public Ship () {
                command = "ship";
                shortHelp = "Ship two people.";
                catagory = Category.Fun;
            }

            [Overload (typeof (Ship), "Ship two people so that they'll be together forever, at least in your headcanon.")]
            public Task<Result> Execute (CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
                Shipping.Ship ship = ParentModule.Ship (data.message.Author as SocketGuildUser, shippieOne, shippieTwo, out bool succesful);
                if (succesful) {
                    return TaskResult (ship, "Succesfully shipped " + shippieOne.GetShownName () + " and " + shippieTwo.GetShownName () + ", now known as " + ship.GetShipName () + ".");
                }
                return TaskResult (ship, $"Failed to ship {shippieOne.GetShownName ()} x {shippieTwo.GetShownName ()} - You've already shipped them.");
            }

            [Overload (typeof (Ship), "Ship two people so that they'll be together forever, with a given custom name.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, string name) {
                ParentModule.NameShip (shippieOne, shippieTwo, name);
                return Execute (data, shippieOne, shippieTwo);
            }

        }

        public class Sink : ModuleCommand<ShippingModule> {

            public Sink() {
                command = "sink";
                shortHelp = "Sink one of your ships.";
                catagory = Category.Fun;
            }

            [Overload (typeof (void), "Sink one of your ships, in case the imaginative spark is gone.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
                Shipping.Ship ship = ParentModule.Sink (data.message.Author as SocketGuildUser, shippieOne, shippieTwo, out bool succesful);
                if (succesful) {
                    return TaskResult (null, "Succesfully sunk " + ship.GetShipName () + ", at least for you.");
                }
                return TaskResult (null, $"Failed to sink {ship.GetShipName ()} - You have not shipped them yet.");
            }
        }

        public class Name : ModuleCommand<ShippingModule> {

            public Name () {
                command = "name";
                shortHelp = "Name a ship something special.";
                catagory = Category.Utility;
            }

            [Overload (typeof (void), "Name a ship something better than the shitty automatic name generator did.")]
            public Task<Result> Execute (CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, string name) {
                ParentModule.NameShip (shippieOne, shippieTwo, name);
                return TaskResult (null, $"{shippieOne.GetShownName ()} x {shippieTwo.GetShownName()} has now been named {name}");
            }

            [Overload (typeof (void), "Reset a ship name back to the automatically generated one.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
                ParentModule.DeleteShipName (shippieOne, shippieTwo);
                Shipping.Ship ship = ParentModule.GetShipByShippies (shippieOne, shippieTwo);
                return TaskResult (null, $"{shippieOne.GetShownName ()} x {shippieTwo.GetShownName ()} has been named back to {ship.GetShipName ()}.");
            }

        }

        public class List : ModuleCommand<ShippingModule> {

            public List () {
                command = "list";
                shortHelp = "List all ships containing a person.";
                catagory = Category.Utility;
            }

            [Overload (typeof (Embed), "List every single ship that you yourself is a part of.")]
            public Task<Result> Execute(CommandMetadata data) {
                return Execute (data, data.message.Author as SocketGuildUser);
            }

            [Overload (typeof (Embed), "List every single ship that the given person is a part of.")]
            public Task<Result> Execute(CommandMetadata data, SocketGuildUser user) {
                var ships = ParentModule.GetShippieShips (user);
                var catagorized = ships.GroupBy (x => x.GetCompanionTo (user.Id));

                EmbedBuilder builder = new EmbedBuilder ();
                builder.WithAuthor (user).
                    WithTitle ("Ships containing " + user.GetShownName () + ".");

                foreach (var ship in catagorized) {
                    var shippers = ship.Select (x => ParentModule.ParentBotClient.GetUser (data.message.GetGuild ().Id, x.Shipper));
                    string shipperNames = "";
                    foreach (var shipper in shippers) {
                        shipperNames += shipper.GetShownName () + "\n";
                    }

                    SocketGuildUser companion = ParentModule.ParentBotClient.GetUser (data.message.GetGuild ().Id, ship.Key);
                    builder.AddField ("Shipped with " + companion.GetShownName () + " as " + ship.FirstOrDefault ().GetShipName () + ", " + ship.Count () + " times by:", shipperNames);
                }

                return TaskResult (builder.Build (), null);
            }

        }

        public class OTPs : ModuleCommand<ShippingModule> {

            public OTPs() {
                command = "otps";
                shortHelp = "Show your true pairs.";
                catagory = Category.Utility;
            }

            [Overload (typeof (string), "Show every single ship that you yourself has shipped.")]
            public Task<Result> Execute(CommandMetadata data) => Execute (data, data.message.Author as SocketGuildUser);

            [Overload (typeof (string), "Show every single ship the given person has shipped.")]
            public Task<Result> Execute (CommandMetadata data, SocketGuildUser user) {
                var allShips = ParentModule.GetAllShipperShips (user);
                string result = $"{user.GetShownName ()} has shipped the following ships:```\n";
                foreach (Shipping.Ship ship in allShips) {
                    result += ship.ToString () + "\n";
                }
                result += "```";
                return TaskResult (result, result);
            }
        }

        public class ATPs : ModuleCommand<ShippingModule> {

            public ATPs() {
                command = "atps";
                shortHelp = "Show a leaderboard of all true pairs.";
                catagory = Category.Utility;
            }

            [Overload (typeof (Embed), "Show a leaderboard of most shipped ships, sorted by most shipped.")]
            public Task<Result> Execute (CommandMetadata data) {
                var leaderboard = ParentModule.GetShipLeaderboard (data.message.GetGuild ());

                EmbedBuilder builder = new EmbedBuilder ().
                    WithAuthor (ParentModule.ParentBotClient.discordClient.CurrentUser).
                    WithTitle ("All ships on " + data.message.GetGuild ().Name + ".");

                foreach (var pair in leaderboard) {
                    SocketGuildUser shippie = ParentModule.ParentBotClient.GetUser (data.message.GetGuild ().Id, pair.Key);
                    string ships = "";

                    foreach (Shipping.Ship ship in pair.Value) {
                        ships += $"{ship.ToString ()}\n";
                    }

                    builder.AddField ($"{shippie.GetShownName ()} has been shipped {pair.Value.Count} times in the following ships:", ships);
                }

                return TaskResult (builder.Build (), null);
            }
        }

    }
}
