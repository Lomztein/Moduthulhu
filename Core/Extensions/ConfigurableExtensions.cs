using Lomztein.Moduthulhu.Core.Configuration;

namespace Lomztein.Moduthulhu.Core.Extensions {

    public static class ConfigurableExtensions {

        internal static void ReloadConfiguration(this IConfigurable configurable) {
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
