using Lomztein.ModularDiscordBot.Core.Bot;
using Lomztein.ModularDiscordBot.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.Configuration {

    /// <summary>
    /// This class might be overengineered, and is subject to change.
    /// </summary>
    public class Config {

        public static string configRootDirectory = AppContext.BaseDirectory + "/Configuration/";

        protected Dictionary<ulong, Dictionary<string, object>> entries = new Dictionary<ulong, Dictionary<string, object>> ();
        protected string name;

        public Config(string _name) {
            name = _name;
        }

        public virtual void Load() {
            try {
                string [ ] files = Directory.GetFiles (GetPath ());
                var loadedEntries = new Dictionary<ulong, Dictionary<string, object>> ();

                foreach (string file in files) {
                    var entry = JSONSerialization.DeserializeFile<Dictionary<string, object>> (file);
                    loadedEntries.Add (ulong.Parse (Path.GetFileNameWithoutExtension (file)), entry);
                }
            } catch (Exception exc) {
                Log.Write (exc);
            }
        }

        public virtual void Save() {
            foreach (var value in entries) {
                JSONSerialization.SerializeObject (entries, GetPath (value.Key), true);
            }
        }

        public T GetEntry<T>(ulong id, string key, T fallback) {
            Log.Write (Log.Type.CONFIG, "Getting config: " + id + "-" + key);
            SetEntryIfEmpty (id, key, fallback);
            return JSONSerialization.ConvertObject<T> (entries[id][key]);
        }

        public void SetEntry(ulong id, string key, object obj) {
            SetEntryIfEmpty (id, key, obj);
            entries [ id ] [ key ] = obj;
        }

        private void SetEntryIfEmpty (ulong id, string key, object fallback) {
            if (!entries.ContainsKey (id))
                entries.Add (id, new Dictionary<string, object> () { { key, fallback } });
            if (!entries [ id ].ContainsKey (key))
                entries [ id ].Add (key, fallback);
        }

        public string GetPath() {
            return configRootDirectory + name;
        }

        public string GetPath(ulong id) {
            if (id == 0) // When there is only one entry, there is no need for multiple files, so no need for folders.
                return GetPath ();

            return configRootDirectory + name + "/" + id.ToString ();
        }
    }
}
