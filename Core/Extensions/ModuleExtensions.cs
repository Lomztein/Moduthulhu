using Lomztein.ModularDiscordBot.Core.Module.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Extensions
{
    public static class ModuleExtensions
    {
        public static string CompactizeName (this IModule module) => module.Author + "." + module.Name;
    }
}
