using Discord.WebSocket;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Lomztein.Moduthulhu.Modules.Misc.Shipping.Commands;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.AdvDiscordCommands.Extensions;

namespace Lomztein.Moduthulhu.Modules.Misc.Shipping {
    // There is arguably a bit of spaghetti in this file, as Ships contain and use references to this parent module quite often.
    // However, it is entirely restricted to this file and the commands file.
    public class ShippingModule : ModuleBase {

        public const string shipFileName = "Ships";
        public const string nameFileName = "Shipnames";

        public override string Name => "Shipping Simulator 2018";
        public override string Description => "At the core of all this lies Senpai.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public Dictionary<ulong, List<Ship>> ships;
        public List<ShipName> shipNames;

        private ShippingCommands commands;

        private void LoadData() {

            ships = DataSerialization.DeserializeData<Dictionary<ulong, List<Ship>>> (shipFileName);
            if (ships == null)
                ships = new Dictionary<ulong, List<Ship>> ();

            foreach (var guild in ships) {
                foreach (Ship ship in guild.Value) {
                    ship.parentModule = this;
                    ship.GuildId = guild.Key;
                }
            }

            shipNames = DataSerialization.DeserializeData<List<ShipName>> (nameFileName);
            if (shipNames == null)
                shipNames = new List<ShipName> ();

        }

        private void SaveData() {
            DataSerialization.SerializeData (ships, shipFileName);
            DataSerialization.SerializeData (shipNames, nameFileName);
        }

        public override void Initialize() {
            LoadData ();
            commands = new ShippingCommands () { ParentModule = this };
            ParentModuleHandler.GetModule<CommandRootModule> ().AddCommands (commands);
        }

        public Ship Ship (SocketGuildUser shipper, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, out bool succesful) {

            Ship ship = new Ship (this, shipper.Guild.Id, shipper.Id, shippieOne.Id, shippieTwo.Id);
            succesful = false;

            if (!ContainsShip (shipper.Guild.Id, ship)) {
                AddShip (shipper.Guild.Id, ship);
                succesful = true;
            }

            return ship;
        }

        public Ship Sink(SocketGuildUser shipper, SocketGuildUser shippieOne, SocketGuildUser shippieTwo, out bool succesful) {
            Ship ship = new Ship (this, shipper.Guild.Id, shipper.Id, shippieOne.Id, shippieTwo.Id);
            succesful = false;

            if (ContainsShip (shipper.Guild.Id, ship)) {
                RemoveShip (shipper.Guild.Id, ship);
                succesful = true;
            }

            return ship;
        }

        public List<Ship> GetAllShipperShips (SocketGuildUser shipper) {

            if (ships.ContainsKey (shipper.Guild.Id)) {
                return ships [ shipper.Guild.Id ].Where (x => x.Shipper == shipper.Id).ToList ();
            }

            return null;
        }

        public Dictionary<ulong, List<Ship>> GetShipLeaderboard(SocketGuild guild) {

            if (ships.ContainsKey (guild.Id)) {

                Dictionary<ulong, List<Ship>> leaderboard = new Dictionary<ulong, List<Ship>> ();
                foreach (Ship ship in ships [ guild.Id ]) {

                    if (leaderboard.ContainsKey (ship.ShippieOne)) {
                        if (!leaderboard [ ship.ShippieOne ].Exists (x => x.IsShippiesIdentical (ship)))
                            leaderboard [ ship.ShippieOne ].Add (ship);
                    } else {
                        leaderboard.Add (ship.ShippieOne, new List<Ship> () { ship });
                    }

                    if (leaderboard.ContainsKey (ship.ShippieTwo)) {
                        if (!leaderboard [ ship.ShippieTwo ].Exists (x => x.IsShippiesIdentical (ship)))
                            leaderboard [ ship.ShippieTwo ].Add (ship);
                    } else {
                        leaderboard.Add (ship.ShippieTwo, new List<Ship> () { ship });
                    }
                }

                var sortingList = leaderboard.ToList ();
                sortingList.Sort ((x, y) => y.Value.Count.CompareTo (x.Value.Count));
                leaderboard = sortingList.ToDictionary (x => x.Key, y => y.Value);
                return leaderboard;

            }

            return null;
        }

        public List<Ship> GetShippieShips(SocketGuildUser shippie) {

            if (ships.ContainsKey (shippie.Guild.Id)) {
                return ships [ shippie.Guild.Id ].Where (x => x.ShippieOne == shippie.Id || x.ShippieTwo == shippie.Id).ToList ();
            }

            return null;
        }

        public bool ContainsShip (ulong guildId, Ship ship) {
            if (ships.ContainsKey (guildId)) {
                if (ships [ guildId ].Contains (ship))
                    return true;
            }
            return false;
        }

        public void AddShip (ulong guildId, Ship ship) {
            if (!ships.ContainsKey (guildId))
                ships.Add (guildId, new List<Ship> ());
            ships [ guildId ].Add (ship);
            SaveData ();
        }

        public void RemoveShip(ulong guildId, Ship ship) {
            if (!ships.ContainsKey (guildId))
                return;
            ships [ guildId ].Remove (ship);
            SaveData ();
        }

        public Ship GetShipByShippies (SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
            if (ships.ContainsKey (shippieOne.Guild.Id)) {
                return ships [ shippieOne.Guild.Id ].Find (x => x.IsShippiesIdentical (shippieOne.Id, shippieTwo.Id));
            }
            return null;
        }

        public void NameShip (SocketGuildUser shippieOne, SocketGuildUser shippieTwo, string name) {
            DeleteShipName (shippieOne, shippieTwo);
            shipNames.Add (new ShipName (shippieOne.Id, shippieTwo.Id, name));
            SaveData ();
        }

        public void DeleteShipName (SocketGuildUser shippieOne, SocketGuildUser shippieTwo) {
            ShipName shipName = GetCustomShipName (shippieOne.Id, shippieTwo.Id);
            if (shipName != null) {
                shipNames.Remove (shipName);
            }
        }

        public ShipName GetCustomShipName (ulong shippieOne, ulong shippieTwo) {
            return shipNames.Find (x => x.shippieOne == shippieOne && x.shippieTwo == shippieTwo);
        }

        public override void Shutdown() {
            ParentModuleHandler.GetModule<CommandRootModule> ().RemoveCommands (commands);
        }

    }

    public class Ship
    {
        [JsonIgnore]
        public ShippingModule parentModule;

        [JsonIgnore]
        public ulong GuildId { get; internal set; }

        public ulong Shipper { get; set; }

        public ulong ShippieOne { get; set; }
        public ulong ShippieTwo { get; set; }

        public Ship(ShippingModule _parentModule, ulong _guildId, ulong _shipper, ulong _shippieOne, ulong _shippieTwo)
        {
            parentModule = _parentModule;
            GuildId = _guildId;
            Shipper = _shipper;

            // Make sure the "orientation" of the ship is set in stone, for consistant ship names.
            ShippieOne = Math.Min(_shippieOne, _shippieTwo);
            ShippieTwo = Math.Max(_shippieOne, _shippieTwo);
        }

        public (SocketGuild guild, SocketGuildUser shipper, SocketGuildUser shippieOne, SocketGuildUser shippeTwo) GetGuildObjects()
        {
            SocketGuild _guild = parentModule.ParentBotClient.GetGuild(GuildId);
            SocketGuildUser _shipper = parentModule.ParentBotClient.GetUser(GuildId, Shipper);
            SocketGuildUser _shippieOne = parentModule.ParentBotClient.GetUser(GuildId, ShippieOne);
            SocketGuildUser _shippieTwo = parentModule.ParentBotClient.GetUser(GuildId, ShippieTwo);
            return (_guild, _shipper, _shippieOne, _shippieTwo);
        }

        public override bool Equals(object obj)
        {
            if (obj is Ship ship)
            {
                return (GuildId == ship.GuildId && Shipper == ship.Shipper && IsShippiesIdentical(ship));
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

        public override int GetHashCode() => 0;

        public override string ToString()
        {
            SocketGuildUser one = parentModule.ParentBotClient.GetUser(GuildId, ShippieOne);
            SocketGuildUser two = parentModule.ParentBotClient.GetUser(GuildId, ShippieTwo);

            return $"{one.GetShownName()} x {two.GetShownName()} - {GetShipName()}";
        }

        public ulong GetCompanionTo(ulong user)
        {
            if (user == ShippieOne)
                return ShippieTwo;
            if (user == ShippieTwo)
                return ShippieOne;
            throw new ArgumentException("Given user is not a shippie in this ship.");
        }

        public string GenerateContractedName()
        {
            SocketGuildUser one = parentModule.ParentBotClient.GetUser(GuildId, ShippieOne);
            SocketGuildUser two = parentModule.ParentBotClient.GetUser(GuildId, ShippieTwo);

            int maxLength = Math.Max(one.GetShownName().Length, two.GetShownName().Length);

            string firstPart = GetPartName(one.GetShownName(), true, maxLength);
            string lastPart = GetPartName(two.GetShownName(), false, maxLength);

            return firstPart + lastPart;
        }

        public string GetShipName()
        {
            ShipName name = parentModule.GetCustomShipName(ShippieOne, ShippieTwo);
            if (name == null)
                return GenerateContractedName();
            return name.shipName;
        }

        private string GetPartName(string fullName, bool firstHalf, int maxLength)
        {
            if (fullName.Length <= maxLength / 2)
            {
                return fullName;
            }

            if (firstHalf)
                return fullName.Substring(0, fullName.Length / 2);
            if (!firstHalf)
                return fullName.Substring(fullName.Length / 2);
            return fullName;
        }
    }

    public class ShipName {

        public ulong shippieOne;
        public ulong shippieTwo;

        public string shipName;

        public ShipName (ulong _shippieOne, ulong _shippieTwo, string _shipName) {

            shippieOne = Math.Min (_shippieOne, _shippieTwo);
            shippieTwo = Math.Max (_shippieOne, _shippieTwo);

            shipName = _shipName;
        }

        public override bool Equals(object obj) {
            if (obj is ShipName shipName) {
                return shippieOne == shipName.shippieOne && shippieTwo == shipName.shippieTwo;
            }
            return false;
        }

        public override int GetHashCode() => 0;

    }
}
