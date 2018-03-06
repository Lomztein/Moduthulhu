using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Modules.CommandRoot;
using Lomztein.Moduthulhu.Modules.Meta.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Modules.Meta.Commands
{
    public class ModuleManagerCommandSet : ModuleCommandSet<ModuleManagerModule>
    {
        public ModuleManagerCommandSet () {
            command = "modules";
            shortHelp = "Get information about modules.";

            commandsInSet = new List<Command> () {
                new List (), new Get (), new Info (),
            };
        }

        public class Get : ModuleCommand<ModuleManagerModule> {

            public Get () {
                command = "get";
                shortHelp = "Get a module object.";
            }

            [Overload (typeof (IModule), "Get a module from the parent manager by name and author.")]
            public Task<Result> Execute (CommandMetadata data, string name, string author) {
                IModule result = parentModule.ParentModuleHandler.GetModule (name, author);
                return TaskResult (result, result.CompactizeName ());
            }

            [Overload (typeof (IModule), "Get a module from the parent manager by seach string.")]
            public Task<Result> Execute(CommandMetadata data, string search) {
                IModule result = parentModule.ParentModuleHandler.GetActiveModules ().Find (x => x.CompactizeName ().Contains (search));
                return TaskResult (result, result.CompactizeName ());
            }

        }

        public class List : ModuleCommand<ModuleManagerModule> {

            public List () {
                command = "list";
                shortHelp = "Display a list of modules.";
            }

            [Overload (typeof (Embed), "Display a list of all currently active modules.")]
            public Task<Result> Execute (CommandMetadata data) {
                return TaskResult (parentModule.ParentModuleHandler.GetModuleListEmbed (), "");
            }

        }

        public class Info : ModuleCommand<ModuleManagerModule> {

            public Info () {
                command = "info";
                shortHelp = "Display information about a module.";
            }

            [Overload (typeof (Embed), "Display information about a specific module.")]
            public Task<Result> Execute (CommandMetadata data, IModule module) {
                return TaskResult (module?.GetModuleEmbed (), "");
            }

            [Overload (typeof (Embed), "Display information about a specific module found by name.")]
            public Task<Result> Execute(CommandMetadata data, string search) {
                IModule module = parentModule.ParentModuleHandler.GetActiveModules ().Find (x => x.CompactizeName ().Contains (search));
                return TaskResult (module?.GetModuleEmbed (), "");
            }
        }
    }
}
