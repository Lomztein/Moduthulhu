using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Plugins.Framework;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Core.Bot;

namespace Lomztein.Moduthulhu.Plugins.Standard

{
    public class AdministratorCommand : PluginCommand<AdministrationPlugin> {

        protected bool IsAdmin(ulong userId) => ParentPlugin.GuildHandler.IsBotAdministrator(userId);

        public override string AllowExecution(CommandMetadata metadata) {
            string baseAllowance = base.AllowExecution (metadata);
            if (!IsAdmin (metadata.AuthorID)) {
                baseAllowance += "\t" + "User is not a bot administrator.";
            }
            return baseAllowance;
        }

    }
}
