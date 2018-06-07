using Lomztein.Moduthulhu.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.Configuration
{
    public class SingleConfig : Config {

        public const ulong SINGLE_ID = 0; 

        public SingleConfig(string _name) : base (_name) { }

        public SingleConfig() : base () { }

        public T GetEntry<T> (string key, T fallback) {
            return GetEntry (SINGLE_ID, key, fallback);
        }

        public void SetEntry (string key, object value, bool save) {
            SetEntry (SINGLE_ID, key, value, save);
        }

        public override void Load() {
            entries = new Dictionary<ulong, Dictionary<string, Entry>> ();
            Dictionary<string, Entry> singleEntries = JSONSerialization.DeserializeFile<Dictionary<string, Entry>> (GetPath ());
            if (singleEntries != null)
                entries.Add (SINGLE_ID, singleEntries);
        }

        public override void Save() {
            JSONSerialization.SerializeObject (entries [SINGLE_ID], GetPath (), true);
            CallOnSaved ();
        }
    }
}
