using System;
using System.Collections.Generic;
using System.Text;
using Lomztein.ModularDiscordBot.Core.Module;

namespace Lomztein.ModularDiscordBot.Modules.CommandRoot
{
    public static class ModuleManagerExtensions
    {
        public static AdvDiscordCommands.Framework.CommandRoot GetCommandRoot (this ModuleHandler handler) {
            return handler.GetModule<CommandRootModule> ().commandRoot;
        }
    }
}
