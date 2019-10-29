using Lomztein.Moduthulhu.Core.Configuration;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.Plugin.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Modules.Meta.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lomztein.Moduthulhu.Core.Configuration.Config;

namespace Lomztein.Moduthulhu.Modules.Meta
{
    public class ConfigurationManagerModule : PluginBase {

        public override string Name => "Configuration Manager";
        public override string Description => "Used for interacting with and changing configuration.";
        public override string Author => "Lomztein";

        public override bool Multiserver => true;

        private ConfigurationManagerCommandSet commandSet;

        public override void Initialize() {
            commandSet = new ConfigurationManagerCommandSet () { ParentModule = this };
            ParentContainer.GetCommandRoot ().AddCommands (commandSet);
        }

        public override void Shutdown() {
            ParentContainer.GetCommandRoot ().RemoveCommands (commandSet);
        }

        public List<IConfigurable> GetModulesWithEntry(ulong id, string key) {

            IPlugin[] allModules = ParentContainer.Modules.ToArray () ;
            List<IConfigurable> withKey = new List<IConfigurable> ();

            foreach (IPlugin module in allModules) {

                if (module is IConfigurable configurableModule) {
                    Config config = configurableModule.GetConfig ();
                    if (config.HasEntry (id, key))
                        withKey.Add (configurableModule);
                }

            }

            return withKey;
        }


        public string ListEntriesInModules(IEnumerable<IPlugin> modules, ulong id, Predicate<Entry> predicate) {

            string list = "";
            foreach (IPlugin module in modules) {

                if (module is IConfigurable configurable) {

                    var entries = configurable.GetConfig ().GetEntryDictionary (id, predicate);

                    if (entries == null)
                        continue;
                    list += module.CompactizeName () + "\n";

                    foreach (var entry in entries) {
                        bool isEnumerable = entry.Value.Object is IEnumerable;
                        string objectString = isEnumerable ? "Enumerable type, use \"!config list\" to see values." : entry.Value.Object.ToString ();

                        list += "\t" + entry.Key.ToString () + " - " + objectString + "\n";
                    }
                    list += "\n";
                }

            }
            return "```" + list + "```";
        }
    }
}
