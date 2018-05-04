using Lomztein.Moduthulhu.Core.Configuration;

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
    }
}
