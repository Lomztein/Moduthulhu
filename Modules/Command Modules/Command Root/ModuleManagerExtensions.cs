using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Module;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;

namespace Lomztein.Moduthulhu.Modules.Command
{
    public static class ModuleManagerExtensions
    {
        public static AdvDiscordCommands.Framework.CommandRoot GetCommandRoot (this ModuleContainer container) {
            return container.GetModule<CommandRootModule> ().commandRoot;
        }
    }
}
