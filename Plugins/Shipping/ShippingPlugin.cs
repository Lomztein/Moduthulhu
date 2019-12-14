using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.Moduthulhu.Core.IO.Database.Repositories;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding.Guild;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Modules.Shipping {

    [Descriptor ("Lomztein", "Shipping Simulator 2018", "At the core of all this lies Guffe.")]
    [GDPR(GDPRCompliance.Partial, "Users may ship each other, thus storing other users ID in a ship.")]
    public class ShippingPlugin : PluginBase {

        private CachedValue<List<Ship>> _ships;
        private CachedValue<List<ShipName>> _shipNames;

        private ShippingCommands _commands;

        public override void Initialize() {
            _commands = new ShippingCommands { ParentPlugin = this };
            SendMessage("Lomztein-Command Root", "AddCommand", _commands);

            _ships = GetDataCache("Ships", x => new List<Ship>());
            _shipNames = GetDataCache("ShipNames", x => new List<ShipName>());
        }

        public bool Ship (ulong shipper, ulong shippieOne, ulong shippieTwo) {

            Ship ship = new Ship (this, GuildHandler.GuildId, shipper, shippieOne, shippieTwo);

            if (!ContainsShip (ship)) {
                AddShip (GuildHandler.GuildId, ship);
                return true;
            }

            return false;
        }

        public bool Sink(ulong shipper, ulong shippieOne, ulong shippieTwo) {
            Ship ship = new Ship (this, GuildHandler.GuildId, shipper, shippieOne, shippieTwo);

            if (ContainsShip (ship)) {
                RemoveShip (GuildHandler.GuildId, ship);
                return true;
            }

            return false;
        }

        public IList<Ship> GetAllShipperShips(SocketGuildUser shipper)
        {
            return _ships.GetValue().Where(x => x.Shipper == shipper.Id).ToList();
        }

        public Dictionary<ulong, List<Ship>> GetShipLeaderboard(SocketGuild guild)
        {
            Dictionary<ulong, List<Ship>> leaderboard = new Dictionary<ulong, List<Ship>>();
            foreach (Ship ship in _ships.GetValue())
            {

                if (leaderboard.ContainsKey(ship.ShippieOne))
                {
                    if (!leaderboard[ship.ShippieOne].Exists(x => x.IsShippiesIdentical(ship)))
                    {
                        leaderboard[ship.ShippieOne].Add(ship);
                    }
                }
                else
                {
                    leaderboard.Add(ship.ShippieOne, new List<Ship> { ship });
                }

                if (leaderboard.ContainsKey(ship.ShippieTwo))
                {
                    if (!leaderboard[ship.ShippieTwo].Exists(x => x.IsShippiesIdentical(ship)))
                    {
                        leaderboard[ship.ShippieTwo].Add(ship);
                    }
                }
                else
                {
                    leaderboard.Add(ship.ShippieTwo, new List<Ship> { ship });
                }
            }

            var sortingList = leaderboard.ToList();
            sortingList.Sort((x, y) => y.Value.Count.CompareTo(x.Value.Count));
            leaderboard = sortingList.ToDictionary(x => x.Key, y => y.Value);
            return leaderboard;

        }
       

        public IList<Ship> GetShippieShips(ulong shippie) {
                return _ships.GetValue().Where (x => x.ShippieOne == shippie || x.ShippieTwo == shippie).ToList ();
        }

        public bool ContainsShip(Ship ship)
        {
            if (_ships.GetValue().Contains(ship))
            {
                return true;
            }
            return false;
        }

        public void AddShip(ulong guildId, Ship ship)
        {
            _ships.MutateValue (x => x.Add(ship));
        }

        public void RemoveShip(ulong guildId, Ship ship) {
            _ships.MutateValue (x => x.Remove (ship));
        }

        public Ship GetShipByShippies(ulong shippieOne, ulong shippieTwo)
        {
            return _ships.GetValue().Find(x => x.IsShippiesIdentical(shippieOne, shippieTwo));
        }

        public void NameShip (ulong shippieOne, ulong shippieTwo, string name) {
            ShipName shipName = new ShipName(shippieOne, shippieTwo, name);
            _shipNames.GetValue().Remove(shipName);
            _shipNames.MutateValue (x => x.Add (shipName));
        }

        public void DeleteShipName (ulong shippieOne, ulong shippieTwo) {
            ShipName shipName = GetCustomShipName (shippieOne, shippieTwo);
            if (shipName != null) {
                _shipNames.MutateValue(x => x.Remove (shipName));
            }
        }

        public ShipName GetCustomShipName (ulong shippieOne, ulong shippieTwo) {
            return _shipNames.GetValue().Find (x => x.ShippieOne == shippieOne && x.ShippieTwo == shippieTwo);
        }

        public string GetShipName(Ship ship)
        {
            ShipName name = GetCustomShipName(ship.ShippieOne, ship.ShippieTwo);
            if (name == null)
            {
                return ship.GenerateContractedName(GuildHandler);
            }
            return name.Name;
        }

        public string ShipToString(Ship ship)
        {
            SocketGuildUser one = GuildHandler.GetUser(ship.ShippieOne);
            SocketGuildUser two = GuildHandler.GetUser(ship.ShippieTwo);

            return $"{one.GetShownName()} x {two.GetShownName()} - {GetShipName(ship)}";
        }

        public override void Shutdown() {
            SendMessage("Lomztein-Command Root", "RemoveCommand", _commands);
        }

        public override JToken RequestUserData(ulong id)
        {
            JArray allShips = new JArray();
            var userShips = GetShippieShips(id);
            foreach (var ship in userShips)
            {
                JObject jShip = new JObject();
                jShip.Add("Ship", JObject.FromObject (ship));

                ShipName shipName = GetCustomShipName(ship.ShippieOne, ship.ShippieTwo);
                if (shipName != null)
                {
                    jShip.Add("CustomName", shipName.Name);
                }
                allShips.Add(jShip);
            }
            return allShips.Count > 0 ? allShips : null;
        }

        public override void DeleteUserData(ulong id)
        {
            var ships = GetShippieShips(id);
            List<ShipName> shipNames = new List<ShipName>();
            foreach (Ship ship in ships)
            {
                ShipName shipName = GetCustomShipName(ship.ShippieOne, ship.ShippieTwo);
                if (shipName != null)
                {
                    shipNames.Add(shipName);
                }
            }

            if (ships.Count > 0)
            {
                _ships.MutateValue(x => x.RemoveAll(y => ships.Contains(y)));
            }

            if (shipNames.Count > 0)
            {
                _shipNames.MutateValue(x => x.RemoveAll(y => shipNames.Contains(y)));
            }
        }
    }

    public class Ship
    {
        [JsonProperty ("Shipper")]
        public ulong Shipper { get; set; }

        [JsonProperty ("ShippieOne")]
        public ulong ShippieOne { get; set; }
        [JsonProperty ("ShippieTwo")]
        public ulong ShippieTwo { get; set; }

        public Ship(ShippingPlugin _parentModule, ulong _guildId, ulong _shipper, ulong _shippieOne, ulong _shippieTwo)
        {
            Shipper = _shipper;

            // Make sure the "orientation" of the ship is set in stone, for consistant ship names.
            ShippieOne = Math.Min(_shippieOne, _shippieTwo);
            ShippieTwo = Math.Max(_shippieOne, _shippieTwo);
        }

        public override bool Equals(object obj)
        {
            if (obj is Ship ship)
            {
                return (Shipper == ship.Shipper && IsShippiesIdentical(ship));
            }
            return false;
        }

        /// <summary>
        /// Only checks the shippies, compared to Equals which compares just about fucking everything.
        /// </summary>
        /// <param name="otherShip"></param>
        /// <returns></returns>
        public bool IsShippiesIdentical(Ship otherShip)
        {
            // Since ship orientation is consinstantly based on user ID's, comparison should be much simpler.
            return IsShippiesIdentical(otherShip.ShippieOne, otherShip.ShippieTwo);
        }

        /// <summary>
        /// Only checks the shippies, compared to Equals which compares just about fucking everything.
        /// </summary>
        /// <param name="_shippieOne"></param>
        /// <param name="_shippieTwo"></param>
        /// <returns></returns>
        public bool IsShippiesIdentical(ulong _shippieOne, ulong _shippieTwo)
        {
            // Since ship orientation is consinstantly based on user ID's, comparison should be much simpler.
            return (ShippieOne == _shippieOne && ShippieTwo == _shippieTwo);
        }

        public override int GetHashCode() => (int)(ShippieOne - ShippieTwo);

        public ulong GetCompanionTo(ulong user)
        {
            if (user == ShippieOne)
            {
                return ShippieTwo;
            }
            if (user == ShippieTwo)
            {
                return ShippieOne;
            }
            throw new ArgumentException("Given user is not a shippie in this ship.");
        }

        public string GenerateContractedName(GuildHandler nameSource)
        {
            SocketGuildUser one = nameSource.GetUser(ShippieOne);
            SocketGuildUser two = nameSource.GetUser(ShippieTwo);

            int maxLength = Math.Max(one.GetShownName().Length, two.GetShownName().Length);

            string firstPart = GetPartName(one.GetShownName(), true, maxLength);
            string lastPart = GetPartName(two.GetShownName(), false, maxLength);

            return firstPart + lastPart;
        }

        private static string GetPartName(string fullName, bool firstHalf, int maxLength)
        {
            if (fullName.Length <= maxLength / 2)
            {
                return fullName;
            }

            if (firstHalf)
            {
                return fullName.Substring(0, fullName.Length / 2);
            }
            else {
                return fullName.Substring(fullName.Length / 2);
            }
        }
    }

    public class ShipName {

            [JsonProperty ("ShippieOne")]
        public ulong ShippieOne { get; private set; }
        [JsonProperty ("ShippieTwo")]
        public ulong ShippieTwo { get; private set; }

            [JsonProperty ("Name")]
        public string Name { get; private set; }

        public ShipName(ulong _shippieOne, ulong _shippieTwo, string _shipName)
        {

            ShippieOne = Math.Min(_shippieOne, _shippieTwo);
            ShippieTwo = Math.Max(_shippieOne, _shippieTwo);

            Name = _shipName;
        }

        public override bool Equals(object obj) {
            if (obj is ShipName shipName) {
                return ShippieOne == shipName.ShippieOne && ShippieTwo == shipName.ShippieTwo;
            }
            return false;
        }

        public override int GetHashCode() => 0;

    }
}
