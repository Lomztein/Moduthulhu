using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Bot;

namespace Lomztein.Moduthulhu.Modules.Administration.AdministrationCommands
{
    public class AdministratorCommand : ModuleCommand<AdministrationModule> {

        // This might not need to be a function, but I'm making certain that it's always getting the correct list by using a func.
        protected Func<UserList> AdministratorSource { get; set; }
        protected string AdministratorTypeName { get; set; }

        public override string AllowExecution(CommandMetadata metadata) {
            string baseAllowance = base.AllowExecution (metadata);
            if (!AdministratorSource ().Contains (metadata.Author)) {
                baseAllowance += "\t" + "User is not a " + AdministratorTypeName + " administrator.";
            }
            return baseAllowance;
        }

    }
}
