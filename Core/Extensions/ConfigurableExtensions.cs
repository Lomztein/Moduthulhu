using Lomztein.Moduthulhu.Core.Configuration;
using System;
using System.Collections.Generic;

namespace Lomztein.Moduthulhu.Core.Extensions {

    public static class ConfigurableExtensions {

        /// <summary>
        /// Use with caution. Whatever this is given must contain a Config object named Configuration.
        /// </summary>
        /// <param name="configurable"></param>
        public static void ReloadConfiguration(this IConfigurable configurable) {
            dynamic dynConfig = configurable;
            dynConfig.Configuration.Load ();
            dynConfig.Configure ();
            dynConfig.Configuration.Save ();
        }

        public static void ReloadConfiguration<T>(this IConfigurable<T> configurable) where T : Config {
            ReloadConfiguration (configurable as IConfigurable);
        }

        public static Config GetConfig(this IConfigurable config) {
            dynamic dynConfig = config;
            return dynConfig.Configuration;
        }

        public static Type GetEntryType (this IConfigurable configurable, ulong id, string key) {
            Config config = configurable.GetConfig ();
            object currentEntry = config.GetEntry (id, key);
            Type type = currentEntry.GetType ();
            return type;
        }

    }
}
