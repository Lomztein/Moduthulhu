using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.Meta.Extensions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lomztein.Moduthulhu.Modules.CustomCommands.Categories;
using Lomztein.AdvDiscordCommands.Framework.Interfaces;

namespace Lomztein.Moduthulhu.Modules.Meta.Commands
{
    public class ModuleManagerCommandSet : ModuleCommandSet<ModuleManagerModule>
    {
        public ModuleManagerCommandSet () {
            Name = "modules";
            Description = "Get module info.";
            Category = AdditionalCategories.Management;

            commandsInSet = new List<ICommand> () {
                new List (), new Get (), new Info (),// new Reload (),
            };
        }

        public class Get : ModuleCommand<ModuleManagerModule> {

            public Get () {
                Name = "get";
                Description = "Get module object.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (IPlugin), "Get a module from the parent manager by name and author.")]
            public Task<Result> Execute (CommandMetadata data, string name, string author) {
                IPlugin result = ParentModule.ParentContainer.Modules.Find (x => x.Name.ToUpper ().Contains (name.ToUpper ()) || x.Author.ToUpper ().Contains (author.ToUpper ()));
                return TaskResult (result, result.CompactizeName ());
            }

            [Overload (typeof (IPlugin), "Get a module from the parent manager by seach string.")]
            public Task<Result> Execute(CommandMetadata data, string search) {
                IPlugin result = ParentModule.ParentContainer.FuzzySearchModule (search);
                return TaskResult (result, result.CompactizeName ());
            }

        }

        public class List : ModuleCommand<ModuleManagerModule> {

            public List () {
                Name = "list";
                Description = "Display module list.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (Embed), "Display a list of all currently active modules.")]
            public Task<Result> Execute (CommandMetadata data) {
                return TaskResult (ParentModule.ParentContainer.GetModuleListEmbed (), "");
            }

        }

        public class Info : ModuleCommand<ModuleManagerModule> {

            public Info () {
                Name = "info";
                Description = "Display module info.";
                Category = AdditionalCategories.Management;
            }

            [Overload (typeof (Embed), "Display information about a specific module.")]
            public Task<Result> Execute (CommandMetadata data, IPlugin module) {
                return TaskResult (module?.GetModuleEmbed (), "");
            }

            [Overload (typeof (Embed), "Display information about a specific module found by name.")]
            public Task<Result> Execute(CommandMetadata data, string search) {
                IPlugin module = ParentModule.ParentContainer.FuzzySearchModule (search);
                return TaskResult (module?.GetModuleEmbed (), "");
            }
        }

        public class Reload : ModuleCommand<ModuleManagerModule> {

            public Reload () {
                Name = "reload";
                Description = "Reload modules.";
                Category = AdditionalCategories.Management;
                CommandEnabled = false;

                RequiredPermissions.Add (GuildPermission.Administrator);
            }

            [Overload (typeof (void), "Reload modules from the module folder.")]
            public Task<Result> Execute (CommandMetadata data) {
                return TaskResult (null, "Modules have been reloaded.");
            }

        }
    }
}
