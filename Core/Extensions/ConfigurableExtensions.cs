using Lomztein.ModularDiscordBot.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Extensions
{
    public static class ConfigurableExtensions
    {
        public static void ReloadConfiguration (this IConfigurable configurable) {
            configurable.Configuration.Load ();
            configurable.Configure ();
        }
    }
}
