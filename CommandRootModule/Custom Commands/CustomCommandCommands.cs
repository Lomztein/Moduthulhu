using Lomztein.AdvDiscordCommands.Framework;
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
            command = "custom";
            catagory = Category.Advanced;
            shortHelp = "Custom command management.";

            requiredPermissions.Add (Discord.GuildPermission.ManageGuild);

            commandsInSet = new List<Command> {
                new Create (),
                new CreateSet (),
                new Remove (),
            };
        }

        public class Create : ModuleCommand<CustomCommandsModule> {

            public Create() {
                command = "create";
                catagory = Category.Advanced;
                shortHelp = "Create a new custom command.";
            }

            [Overload (typeof (CustomCommand), "Create a new custom commands set.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, CustomCommandSet commandSet, string commandChain) => Execute (metadata, name, description, "Private", "Miscilaneous", commandSet, commandChain);

            [Overload (typeof (CustomCommand), "Create a new custom commands set with a specfic accessability.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, string accessability, CustomCommandSet commandSet, string commandChain) => Execute (metadata, name, description, accessability, "Miscilaneous", commandSet, commandChain);

            [Overload (typeof (CustomCommand), "Create a new custom command.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, string accessability, string category, CustomCommandSet commandSet, string commandChain) {

                if (!commandSet.ContainsCommandByName (name)) {
                    CommandAccessability commandAccessability = (CommandAccessability)Enum.Parse (typeof (CommandAccessability), accessability, true);
                    Category commandCategory = (Category)Enum.Parse (typeof (Category), category, true);
                    CustomCommand command = CustomCommandsModule.CreateCommand (name, description, metadata.message.Author, commandAccessability, commandCategory, commandChain);
                    commandSet.AddCommands (command);

                    ParentModule.SaveData ();
                    return TaskResult (command, "Succesfully created new command.");
                } else {
                    return TaskResult (command, $"Failed to create command - A command by name **{name}** already exists in the {commandSet.ToString ()} set.");
                }

            }
        }

        public class CreateSet : ModuleCommand<CustomCommandsModule> {

            public CreateSet() {
                command = "createset";
                catagory = Category.Advanced;
                shortHelp = "Create a new custom command set.";
            }

            [Overload (typeof (CustomCommandSet), "Create a new custom commands set.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description) => Execute (metadata, name, description, "Private", "Miscilaneous");

            [Overload (typeof (CustomCommandSet), "Create a new custom commands set with a specfic accessability.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, string accessability) => Execute (metadata, name, description, accessability, "Miscilaneous");

            [Overload (typeof (CustomCommandSet), "Create a new custom command set with a specific accessability and catagory.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, string description, string accessability, string category) {
                CommandAccessability commandAccessability = (CommandAccessability)Enum.Parse (typeof (CommandAccessability), accessability, true);
                Category commandCategory = (Category)Enum.Parse (typeof (Category), category, true);

                CustomCommandSet newCommandSet = CustomCommandsModule.CreateSet (name, description, metadata.message.Author, commandAccessability, commandCategory);
                metadata.root.AddCommands (newCommandSet);
                ParentModule.AddSetToList (newCommandSet);

                ParentModule.SaveData ();

                return TaskResult (newCommandSet, "Succesfully created new command set.");
            }
        }

        public class Remove : ModuleCommand<CustomCommandsModule> {

            public Remove() {
                command = "remove";
                catagory = Category.Advanced;
                shortHelp = "Remove a custom command.";
            }

            [Overload (typeof (void), "Remove the first found command in the given set that matches the given name.")]
            public Task<Result> Execute(CommandMetadata metadata, string name, ICommandSet commandSet) {
                if (commandSet.ContainsCommandByName (name)) {
                    ParentModule.RemoveCustomCommand (commandSet, name);

                    if (commandSet.GetCommands ().Count == 0)
                        metadata.root.RemoveCommands (commandSet as Command);

                    return TaskResult (true, $"Succesfully removed command **{name}** from **{commandSet.ToString ()}**");
                } else {
                    return TaskResult (true, $"Failed to remove command **{name}** from **{commandSet.ToString ()}** - It doesn't exist.");
                }

            }
        }
    }
}
