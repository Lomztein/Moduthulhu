using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.CustomCommands.Commands
{
    public class CustomCommandCommands : ModuleCommandSet<CustomCommandsModule>
    {
        public CustomCommandCommands () {
            Name = "custom";
            Category = StandardCategories.Advanced;
            Description = "Custom command management.";

            RequiredPermissions.Add (GuildPermission.ManageGuild);

            commandsInSet = new List<ICommand> {
                new Create (),
                new CreateSet (),
                new Remove (),
            };
        }

        public class Create : ModuleCommand<CustomCommandsModule> {

            public Create() {
                Name = "create";
                Description = "Create a new custom command.";
                Category = StandardCategories.Advanced;
            }

            [Overload (typeof (CustomCommand), "Create a new custom commands.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, CustomCommandSet commandSet, string commandChain) => Execute (metadata, name, description, "Private", commandSet, commandChain);

            [Overload (typeof (CustomCommand), "Create a new custom command with a specific accesability.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, string accessability, CustomCommandSet commandSet, string commandChain) {

                if (!commandSet.ContainsCommandByName (name)) {
                    CommandAccessability commandAccessability = (CommandAccessability)Enum.Parse (typeof (CommandAccessability), accessability, true);
                    CustomCommand command = CustomCommandsModule.CreateCommand (name, description, metadata.Message.Author, commandAccessability, commandChain);
                    commandSet.AddCommands (command);

                    ParentModule.SaveData ();
                    return TaskResult (command, "Succesfully created new command.");
                } else {
                    return TaskResult (null, $"Failed to create command - A command by name **{name}** already exists in the {commandSet.ToString ()} set.");
                }

            }
        }

        public class CreateSet : ModuleCommand<CustomCommandsModule> {

            public CreateSet() {
                Name = "createset";
                Description = "Create a new custom command set.";
                Category = StandardCategories.Advanced;
            }

            [Overload (typeof (CustomCommandSet), "Create a new custom commands set.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description) => Execute (metadata, name, description, "Private");

            [Overload (typeof (CustomCommandSet), "Create a new custom command set with a specific accessability.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, string accessability) {
                CommandAccessability commandAccessability = (CommandAccessability)Enum.Parse (typeof (CommandAccessability), accessability, true);

                CustomCommandSet newCommandSet = CustomCommandsModule.CreateSet (name, description, metadata.Message.Author, commandAccessability);
                metadata.Root.AddCommands (newCommandSet);
                ParentModule.AddSetToList (newCommandSet);

                ParentModule.SaveData ();

                return TaskResult (newCommandSet, "Succesfully created new command set.");
            }
        }

        public class Remove : ModuleCommand<CustomCommandsModule> {

            public Remove() {
                Name = "remove";
                Description = "Remove a custom command.";
                Category = StandardCategories.Advanced;
            }

            [Overload (typeof (void), "Remove the first found command in the given set that matches the given name.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, ICommandSet commandSet) {
                if (commandSet.ContainsCommandByName (name)) {
                    ParentModule.RemoveCustomCommand (commandSet, name);

                    if (commandSet.GetCommands ().Count == 0)
                        metadata.Root.RemoveCommands (commandSet as Command);

                    return TaskResult (true, $"Succesfully removed command **{name}** from **{commandSet.ToString ()}**");
                } else {
                    return TaskResult (true, $"Failed to remove command **{name}** from **{commandSet.ToString ()}** - It doesn't exist.");
                }

            }
        }
    }
}
