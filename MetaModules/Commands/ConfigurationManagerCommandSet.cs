using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Configuration.Management;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Meta.Commands {

    class ConfigurationManagerCommandSet : ModuleCommandSet<ConfigurationManagerModule> {

        public ConfigurationManagerCommandSet() {
            command = "config";
            shortHelp = "Commands for configuring the bot.";
            catagory = Category.Admin;
            requiredPermissions.Add (Discord.GuildPermission.ManageGuild);

            commandsInSet = new List<Command> () {
                new Change (), new Add (), new Remove (),
                new List (), new All (), new Unset (),
            };
        }

        protected static bool ReturnErrorIfMulitple (List<IConfigurable> configurables, string key, out Task<Result> result) {
            result = null;
            if (configurables.Count > 1) {
                string allNames = configurables.Select (x => (x as IModule).CompactizeName ()).ToArray ().Singlify ();
                result = Task.FromResult (new Result (null, $"Error - Multiple modules contains key \"{key}\", please try again while specifying between the following: {allNames}"));

                return true ;
            }
            return false;
        }

        public class Change : ModuleCommand<ConfigurationManagerModule> {

            public Change() {
                command = "change";
                shortHelp = "Change a specific key.";
                catagory = Category.Admin;
                requiredPermissions.Add (Discord.GuildPermission.ManageGuild);
            }

            [Overload (typeof (void), "Change a specific key in the given IConfigurable.")]
            public Task<Result> Execute(CommandMetadata data, IConfigurable configurable, string key, params string[] inputValues) {
                configurable.ChangeEntry (data.message.GetGuild ().Id, key, true, true, inputValues);
                return TaskResult (null, $"Succesfully set \"{key}\" to {inputValues.Singlify ()}");
            }

            [Overload (typeof (void), "Change a specific key in a searched for module.")]
            public Task<Result> Execute(CommandMetadata data, string moduleSearch, string key, params string[] inputValues) {
                if (ParentModule.ParentModuleHandler.FuzzySearchModule (moduleSearch) is IConfigurable configurable)
                    return Execute (data, configurable, key, inputValues);
                return TaskResult (null, $"Error - No configurable module was found when \"" + moduleSearch + "\" was searched.");
            }

            [Overload (typeof (void), "Change a specified configuration key.")]
            public Task<Result> Execute(CommandMetadata data, string key, params string[] inputValues) {
                var configurables = ParentModule.GetModulesWithEntry (data.message.GetGuild ().Id, key);
                if (ReturnErrorIfMulitple (configurables, key, out var result))
                    return result;
                return Execute (data, configurables.Single (), key, inputValues);
            }
        }

            public class Add : ModuleCommand<ConfigurationManagerModule> {

            public Add () {
                command = "add";
                shortHelp = "Add a value to a list-type entry.";
                catagory = Category.Admin;
                requiredPermissions.Add (Discord.GuildPermission.ManageGuild);
            }

            [Overload (typeof (void), "Add a value to a list/enumerable type configuration entry in the given IConfigurable.")]
            public Task<Result> Execute (CommandMetadata data, IConfigurable configurable, string key, params string[] inputValues) {
                configurable.AddToEntry ( data.message.GetGuild ().Id, key, true, true, inputValues);
                return TaskResult (null, $"Succesfully added \"{inputValues.Singlify ()}\" to \"{key}\"");
            }

            [Overload (typeof (void), "Add to a specific key in a searched for module.")]
            public Task<Result> Execute(CommandMetadata data, string moduleSearch, string key, params string[] inputValues) {
                if (ParentModule.ParentModuleHandler.FuzzySearchModule (moduleSearch) is IConfigurable configurable)
                    return Execute (data, configurable, key, inputValues);
                return TaskResult (null, $"Error - No configurable module was found when \"" + moduleSearch + "\" was searched.");
            }

            [Overload (typeof (void), "Add to a specified configuration key.")]
            public Task<Result> Execute(CommandMetadata data, string key, params string[] inputValues) {
                var configurables = ParentModule.GetModulesWithEntry (data.message.GetGuild ().Id, key);
                if (ReturnErrorIfMulitple (configurables, key, out var result))
                    return result;
                return Execute (data, configurables.Single (), key, inputValues);
            }
        }

        public class Remove : ModuleCommand<ConfigurationManagerModule> {

            public Remove() {
                command = "remove";
                shortHelp = "Remove a value from a list-type entry.";
                catagory = Category.Admin;
                requiredPermissions.Add (Discord.GuildPermission.ManageGuild);
            }

            [Overload (typeof (void), "Remove a value from a list/enumerable type configuration entry in the given IConfigurable.")]
            public Task<Result> Execute(CommandMetadata data, IConfigurable configurable, string key, int index) {
                object obj = configurable.RemoveFromEntry (data.message.GetGuild ().Id, key, true, true, index);
                return TaskResult (null, $"Succesfully removed \"{obj}\" from \"{key}\"");
            }

            [Overload (typeof (void), "Remove value from a specific key in a searched for module.")]
            public Task<Result> Execute(CommandMetadata data, string moduleSearch, string key, int index) {
                if (ParentModule.ParentModuleHandler.FuzzySearchModule (moduleSearch) is IConfigurable configurable)
                    return Execute (data, configurable, key, index);
                return TaskResult (null, $"Error - No configurable module was found when \"" + moduleSearch + "\" was searched.");
            }

            [Overload (typeof (void), "Remove a value from a specified configuration key.")]
            public Task<Result> Execute(CommandMetadata data, string key, int index) {
                var configurables = ParentModule.GetModulesWithEntry (data.message.GetGuild ().Id, key);
                if (ReturnErrorIfMulitple (configurables, key, out var result))
                    return result;
                return Execute (data, configurables.Single (), key, index);
            }
        }

        public class List : ModuleCommand<ConfigurationManagerModule> {

            public List () {
                command = "list";
                shortHelp = "List all values in a list-type entry.";
                catagory = Category.Admin;
                requiredPermissions.Add (Discord.GuildPermission.ManageGuild);
            }

            [Overload (typeof (string), "List every value in a list/enumerable type configuration entry in the given IConfigurable.")]
            public Task<Result> Execute (CommandMetadata data, IConfigurable configurable, string key) {
                string list = "```" + configurable.ListToString (data.message.GetGuild ().Id, key) + "```";
                return TaskResult (list, list);
            }


            [Overload (typeof (string), "List every value in a list/enumerable type configuration entry in the search module.")]
            public Task<Result> Execute(CommandMetadata data, string moduleSearch, string key) {
                if (ParentModule.ParentModuleHandler.FuzzySearchModule (moduleSearch) is IConfigurable configurable)
                    return Execute (data, configurable, key);
                return TaskResult (null, $"Error - No configurable module was found when \"" + moduleSearch + "\" was searched.");
            }

            [Overload (typeof (string), "List every value in a list/enumerable type configuration entry.")]
            public Task<Result> Execute(CommandMetadata data, string key) {
                var configurables = ParentModule.GetModulesWithEntry (data.message.GetGuild ().Id, key);
                if (ReturnErrorIfMulitple (configurables, key, out var result))
                    return result;
                return Execute (data, configurables.Single (), key);
            }

        }

        public class All : ModuleCommand<ConfigurationManagerModule> {

            public All() {
                command = "all";
                shortHelp = "Show every single configuration entry.";
                catagory = Category.Admin;
                requiredPermissions.Add (Discord.GuildPermission.ManageGuild);
            }

            [Overload (typeof (string), "List every single configuration entry in all modules.")]
            public Task<Result> Execute (CommandMetadata data) {
                string all = ParentModule.ListEntriesInModules (ParentModule.ParentModuleHandler.GetActiveModules (), data.message.GetGuild ().Id, x => true);
                return TaskResult (all, all);
            }

            [Overload (typeof (string), "List every value in a list/enumerable type configuration entry in the search module.")]
            public Task<Result> Execute(CommandMetadata data, string moduleSearch) {
                if (ParentModule.ParentModuleHandler.FuzzySearchModule (moduleSearch) is IConfigurable configurable) {
                    string all = ParentModule.ListEntriesInModules (new IModule[] { configurable as IModule }, data.message.GetGuild ().Id, x => true);
                    return TaskResult (all, all);
                }
                return TaskResult (null, $"Error - No configurable module was found when \"" + moduleSearch + "\" was searched.");
            }
        }

        public class Unset : ModuleCommand<ConfigurationManagerModule> {

            public Unset() {
                command = "unset";
                shortHelp = "Show every configuration entry that hasn't been set.";
                catagory = Category.Admin;
                requiredPermissions.Add (Discord.GuildPermission.ManageGuild);
            }

            [Overload (typeof (string), "List every single configuration entry in all modules that hasn't been manually set.")]
            public Task<Result> Execute(CommandMetadata data) {
                string all = ParentModule.ListEntriesInModules (ParentModule.ParentModuleHandler.GetActiveModules (), data.message.GetGuild ().Id, x => !x.ManuallySet);
                return TaskResult (all, all);
            }

            [Overload (typeof (string), "List every value in a list/enumerable type configuration entry in the search module.")]
            public Task<Result> Execute(CommandMetadata data, string moduleSearch) {
                if (ParentModule.ParentModuleHandler.FuzzySearchModule (moduleSearch) is IConfigurable configurable) {
                    string all = ParentModule.ListEntriesInModules (new IModule[] { configurable as IModule }, data.message.GetGuild ().Id, x => true);
                    return TaskResult (all, all);
                }
                return TaskResult (null, $"Error - No configurable module was found when \"" + moduleSearch + "\" was searched.");
            }
        }

    }
}
