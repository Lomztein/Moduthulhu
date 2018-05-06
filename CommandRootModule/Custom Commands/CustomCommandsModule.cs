using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.CustomCommands.Commands;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lomztein.Moduthulhu.Modules.CustomCommands
{
    public class CustomCommandsModule : ModuleBase {

        public const string customCommandFileName = "CustomCommands";

        public override string Name => "Custom Commands";
        public override string Description => "Allows for the packaging of long command chains into a single command. A method, if you will.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        public List<CustomSetData> customCommandData = new List<CustomSetData> ();
        public List<CustomCommandSet> customSets = new List<CustomCommandSet> ();

        private CustomCommandCommands customCommandCommands;
        private CommandRootModule commandRootModule;

        public override void Initialize() {
            customCommandCommands = new CustomCommandCommands () { ParentModule = this };
            commandRootModule = ParentModuleHandler.GetModule<CommandRootModule> ();
            commandRootModule.AddCommands (customCommandCommands);
            LoadData ();
        }

        public override void Shutdown() {
            ParentModuleHandler.GetModule<CommandRootModule> ().RemoveCommands (customCommandCommands);
            commandRootModule.RemoveCommands (customSets.ToArray ());
        }

        public void LoadData() {
            commandRootModule.RemoveCommands (customSets.ToArray ()); // Just in case we need to reload data at some point.
            customCommandData = DataSerialization.DeserializeData<List<CustomSetData>> (customCommandFileName);
            foreach (var set in customCommandData) {
                customSets.Add (set.CreateFrom () as CustomCommandSet);
            }
            commandRootModule.AddCommands (customSets.ToArray ());
        }

        public void SaveData () {
            customCommandData = new List<CustomSetData> ();
            foreach (var cmd in customSets) {
                CustomSetData data = cmd.SaveToData () as CustomSetData;
                customCommandData.Add (data);
            }
            DataSerialization.SerializeData (customCommandData, customCommandFileName);
        }

        public static CustomCommand CreateCommand (string name, string description, IUser author, CommandAccessability accessability, Command.Category category, string commandChain) {
            CustomCommand command = new CustomCommand {
                commandChain = commandChain
            };

            SetCommandData (command, name, description, author, accessability, category);
            return command;
        }

        public static CustomCommandSet CreateSet (string name, string description, IUser author, CommandAccessability accessability, Command.Category category) {
            CustomCommandSet commandSet = new CustomCommandSet ();
            SetCommandData (commandSet, name, description, author, accessability, category);
            return commandSet;
        }

        public void AddSetToList(CustomCommandSet set) => customSets.Add (set);

        private static void SetCommandData (Command command, string name, string description, IUser author, CommandAccessability accessability, Command.Category category) {

            command.catagory = Command.Category.Advanced;
            command.shortHelp = description;
            command.command = name;
            command.catagory = category;

            ICustomCommand custom = command as ICustomCommand;

            custom.Accessability = accessability;

            if (accessability == CommandAccessability.Private)
                custom.OwnerID = author.Id;

            if (accessability == CommandAccessability.Public) {
                if (author is SocketGuildUser guildUser) {
                    custom.OwnerID = guildUser.Guild.Id;
                } else {
                    return;
                }
            }

            return;
        }

        public void RemoveCustomCommand (ICommandSet fromSet, string commandName) {

            var commands = fromSet.GetCommands ();
            Command toRemove = null;

            foreach (var cmd in commands) {
                if (cmd is ICustomCommand customCommand) {

                    if (cmd.command == commandName)
                        toRemove = cmd;

                }
            }

            fromSet.RemoveCommands (toRemove);
        }
    }
}
