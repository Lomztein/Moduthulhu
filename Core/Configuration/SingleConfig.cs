using Lomztein.ModularDiscordBot.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration
{
    public class SingleConfig : Config {

        public SingleConfig(string _name) : base (_name) { }

        public T GetEntry<T> (string key, T fallback) {
            return GetEntry (0, key, fallback);
        }

        public override void Load() {
            entries = JSONSerialization.DeserializeFile<Dictionary<ulong, Dictionary<string, object>>> (GetPath ());
            if (entries == null)
                entries = new Dictionary<ulong, Dictionary<string, object>> ();
        }

        public override void Save() {
            JSONSerialization.SerializeObject (entries, GetPath (), true);
        }
    }
}
