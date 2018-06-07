using Lomztein.Moduthulhu.Core.Bot;
using Lomztein.Moduthulhu.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;

namespace Lomztein.Moduthulhu.Core.Configuration {

    /// <summary>
    /// This class might be overengineered, and is subject to change.
    /// </summary>
    public abstract class Config {

        public static string configRootDirectory = AppContext.BaseDirectory + "/Configuration/";

        protected Dictionary<ulong, Dictionary<string, Entry>> entries = new Dictionary<ulong, Dictionary<string, Entry>> ();

        public string Name { get; internal set; }

        public delegate void SavedEvent();
        public event SavedEvent OnSaved;

        public Config(string _name) {
            Name = _name;
        }

        public Config() {
            Name = "Unnamed Config";
        }

        public abstract void Load();

        public abstract void Save();

        internal void CallOnSaved () {
            OnSaved?.Invoke ();
        }

        public Dictionary<string, Entry> GetEntryDictionary(ulong id, Predicate<Entry> filter) {
            Dictionary<string, Entry> nonSet = new Dictionary<string, Entry> ();

            if (!entries.ContainsKey (id))
                return null;

            foreach (var entry in entries[id]) {
                if (filter (entry.Value))
                    nonSet.Add (entry.Key, entry.Value);
            }
            return nonSet;
        }

        public T GetEntry<T>(ulong id, string key, T fallback) {
            Log.Write (Log.Type.CONFIG, "Getting config: " + id + "-" + key);
            SetEntryIfEmpty (id, key, fallback);
            return GetEntry<T> (id, key);
        }

        public bool HasEntry (ulong id, string key) {
            if (!entries.ContainsKey (id))
                return false;
            if (!entries[id].ContainsKey (key))
                return false;
            return true;
        }

        public T GetEntry<T> (ulong id, string key) {
            if (entries[id][key].Converted == true)
                return (T)entries[id][key].Object;

            object value = JSONSerialization.ConvertObject<T> (entries[id][key].Object);
            entries[id][key].Object = value;
            entries[id][key].Converted = true;
            return (T)entries[id][key].Object;
        }

        public object GetEntry (ulong id, string key) {
            if (entries[id][key].Converted == true)
                return entries[id][key].Object;
            throw new InvalidOperationException ("Object at \"" + key + "\" has not yet been converted from JSON format.");
        }

        public object GetEntry(ulong id, string key, object fallback) {
            SetEntryIfEmpty (id, key, fallback);
            object obj = entries[id][key].Object;
            Type convertTo = fallback.GetType ();

            obj = JSONSerialization.ConvertObject (obj, convertTo);

            entries[id][key].Converted = true;
            entries[id][key].Object = obj;
            return entries[id][key].Object;
        }

        public Entry GetRawEntry (ulong id, string key) {
            return entries[id][key];
        }

        public void SetEntry(ulong id, string key, object obj, bool save, bool manual = false) {

            SetEntryIfEmpty (id, key, obj);
            entries [ id ] [ key ] = new Entry (obj);

            if (manual == true)
                entries[id][key].ManuallySet = true;

            if (save)
                Save ();
        }

        private void SetEntryIfEmpty (ulong id, string key, object fallback) {
            if (!entries.ContainsKey (id)) {
                entries.Add (id, new Dictionary<string, Entry> ());
            }

            if (!entries [ id ].ContainsKey (key)) {
                entries [ id ].Add (key, new Entry(fallback));
            }
        }

        public string GetPath() {
            return configRootDirectory + Name;
        }

        public class Entry {

            public object Object { get; set; }
            public bool ManuallySet { get; internal set; }

            [JsonIgnore] public bool Converted { get; internal set; }

            public Entry (object obj) {
                Object = obj;
                ManuallySet = false;
                Converted = false;
            }

        }
    }
}
