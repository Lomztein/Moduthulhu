using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Lomztein.ModularDiscordBot.Core.Bot;

namespace Lomztein.ModularDiscordBot.Core.IO
{
    /// <summary>
    /// Various JSON related serialization methods. Using the IO methods force the file extension to .json, for consistancy.
    /// </summary>
    public static class JSONSerialization {

        private const string jsonExtension = ".json";

        public static T DeserializeFile<T>(string path) {
            path = Path.ChangeExtension (path, jsonExtension);
            Log.Write (Log.Type.SYSTEM, "Loading JSON file at " + path);

            if (File.Exists (path)) {
                try {
                    string content = File.ReadAllText (path);
                    return JsonConvert.DeserializeObject<T> (content);
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            return default (T);
        }

        public static void SerializeObject (object obj, string path, bool format = false) {
            path = Path.ChangeExtension (path, jsonExtension);
            Directory.CreateDirectory (Path.GetDirectoryName (path));

            Log.Write (Log.Type.SYSTEM, "Saving JSON file at " + path);

            try {
                string json = JsonConvert.SerializeObject (obj, format ? Formatting.Indented : Formatting.None);
                File.WriteAllText (path, json);
            } catch (Exception exc) {
                Log.Write (exc);
            }
        }

        public static T ConvertObject<T>(object input) {
            object obj;
            try {
                try {
                    obj = (T)Convert.ChangeType (input, typeof (T));
                } catch {
                    obj = (T)input;
                }
            } catch {
                string possibleJSON = input.ToString ();
                obj = JsonConvert.DeserializeObject<T> (possibleJSON);
            }

            return (T)obj;
        }
    }
}
