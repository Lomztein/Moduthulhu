using Discord;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Core.IO;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.CustomCommands.Commands;
using Lomztein.Moduthulhu.Modules.CustomCommands.Data;
using Lomztein.Moduthulhu.Modules.CustomCommands.IO;
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

        public List<ICustomCommand> customCommands = new List<ICustomCommand> ();

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
            commandRootModule.RemoveCommands (customCommands.ToArray ());
        }

        public void LoadData() {
            AddCommands (CustomCommandIO.LoadAll (CustomCommandIO.DataPath));
        }

        public void SaveData () {
            CustomCommandIO.SaveAll (customCommands.ToArray (), CustomCommandIO.DataPath);
        }

        public static CustomCommand CreateCommand (string name, string description, IUser author, CommandAccessability accessability, string commandChain) {
            CustomCommand command = new CustomCommand {
                commandChain = commandChain
            };

            SetCommandData (command, name, description, author, accessability);
            return command;
        }

        public static CustomCommandSet CreateSet (string name, string description, IUser author, CommandAccessability accessability) {
            CustomCommandSet commandSet = new CustomCommandSet ();
            SetCommandData (commandSet, name, description, author, accessability);
            return commandSet;
        }

        public void AddCommands(params ICustomCommand[] commands) {
            customCommands.AddRange (commands);
            ParentModuleHandler.GetModule<CommandRootModule> ().AddCommands (commands);
            SaveData ();
        }

        private static void SetCommandData (AdvDiscordCommands.Framework.Command command, string name, string description, IUser author, CommandAccessability accessability) {

            command.Description = description;
            command.Name = name;

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
            ICommand toRemove = null;

            foreach (var cmd in commands) {
                if (cmd is ICustomCommand customCommand) {

                    if (cmd.Name == commandName)
                        toRemove = cmd;
                }
            }

            fromSet.RemoveCommands (toRemove);
        }
    }
}
