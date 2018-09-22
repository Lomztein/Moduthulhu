using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Module;

namespace Lomztein.Moduthulhu.Modules.Command
{
    public static class ModuleManagerExtensions
    {
        public static AdvDiscordCommands.Framework.CommandRoot GetCommandRoot (this ModuleLoader handler) {
            return handler.GetModule<CommandRootModule> ().commandRoot;
        }
    }
}
