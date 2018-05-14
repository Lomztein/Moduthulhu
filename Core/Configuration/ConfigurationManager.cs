using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration
{
    public class ConfigurationManager {

        internal List<IConfigurable> AllConfigurables { get; private set; } = new List<IConfigurable> ();

        public void ChangeEntry (IConfigurable config, ulong id, string entry, object newValue, bool save) {
            dynamic dynConfig = config;
            Config configuration = dynConfig.Configuration;
            configuration.SetEntry (id, entry, newValue, save);
        }

        public void AddToArrayEntry (IConfigurable config, ulong id, string entry, object newValue, bool save) {

        }

    }
}
