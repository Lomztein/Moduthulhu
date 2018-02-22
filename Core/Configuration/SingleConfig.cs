using Lomztein.ModularDiscordBot.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration
{
    public class SingleConfig : Config {

        public SingleConfig(string _name) : base (_name) { }

        public SingleConfig() : base () { }

        public T GetEntry<T> (string key, T fallback) {
            return GetEntry (0, key, fallback);
        }

        public void SetEntry (string key, object value, bool save) {
            SetEntry (0, key, value, save);
        }

        public override void Load() {
            entries = new Dictionary<ulong, Dictionary<string, object>> ();
            Dictionary<string, object> singleEntries = JSONSerialization.DeserializeFile<Dictionary<string, object>> (GetPath ());
            if (singleEntries != null)
                entries.Add (0, singleEntries);
        }

        public override void Save() {
            JSONSerialization.SerializeObject (entries[0], GetPath (), true);
        }
    }
}
