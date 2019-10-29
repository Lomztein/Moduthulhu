using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.Moduthulhu.Core.Plugin;
using Lomztein.Moduthulhu.Core.Bot.Client.Sharding;

namespace Lomztein.Moduthulhu.Modules.Command
{
    public static class ModuleManagerExtensions
    {
        public static AdvDiscordCommands.Framework.CommandRoot GetCommandRoot (this PluginManager container) {
            return container.GetModule<CommandRootModule> ().commandRoot;
        }
    }
}
