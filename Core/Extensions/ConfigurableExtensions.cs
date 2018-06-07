using Lomztein.Moduthulhu.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Lomztein.Moduthulhu.Core.Extensions {

    public static class ConfigurableExtensions {

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

        public static void AutoConfigure (this IConfigurable configurable, List<SocketGuild> guilds) {

            Type type = configurable.GetType ();
            Config config = configurable.GetConfig ();

            IEnumerable<FieldInfo> fields = GetAutoConfigFields (type);

            foreach (FieldInfo field in fields) {
                var value = field.GetValue (configurable) as EntryBase;
                value.ParentConfig = config;

                Type valueType = value.GetType ().GetGenericArguments ()[0]; // Get the value type of the entry.

                if (config is SingleConfig singleConfig) {
                    SetEntryFromDefaultIfNotSaved (value, guilds[0], SingleConfig.SINGLE_ID);
                }

                if (config is MultiConfig multiConfig) {
                    foreach (SocketGuild guild in guilds) {
                        SetEntryFromDefaultIfNotSaved (value, guild, guild.Id);
                    }
                }
            }

        }

        // I don't want the world to see this name.
        internal static void SetEntryFromDefaultIfNotSaved<TSource> (EntryBase entryBase, TSource defaultSource, ulong id) where TSource : IEntity<ulong> {
            dynamic dynEntry = entryBase;

            object defaultValue = dynEntry.GetDefault (defaultSource);
            object entryValue = entryBase.ParentConfig.GetEntry (id, entryBase.Key, defaultValue);

            entryBase.SetEntry (defaultSource.Id, entryValue);
        }

        internal static IEnumerable<FieldInfo> GetAutoConfigFields(Type type) => type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where (x => x.IsDefined (typeof (AutoConfigAttribute), true)).ToList ();

        public static EntryBase GetConfigEntry(this IConfigurable configurable, string key) {
            Type type = configurable.GetType ();
            var fields = GetAutoConfigFields (type);

            foreach (FieldInfo field in fields) {
                var value = field.GetValue (configurable) as EntryBase;
                if (value.Key == key)
                    return value;
            }
            return null;
        }

        public static bool IsConfigured (this IConfigurable configurable, ulong id) {
            Type type = configurable.GetType ();
            var fields = GetAutoConfigFields (type);

            foreach (FieldInfo field in fields) {
                var value = field.GetValue (configurable) as EntryBase;
                if (value.IsCritical == true) {

                    Config config = configurable.GetConfig ();
                    if (config.GetRawEntry (id, value.Key).ManuallySet == false)
                        return false;
                }
            }

            return true;
        }

    }
}
