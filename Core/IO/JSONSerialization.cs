using System;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Core.IO
{
    /// <summary>
    /// Various JSON related serialization methods. Using the IO methods force the file extension to .json, for consistancy.
    /// </summary>
    public static class JSONSerialization {

        private const string jsonExtension = ".json";

        public static T DeserializeFile<T>(string path) => DeserializeFile<T> (path, null);

        public static T DeserializeFile<T>(string path, JsonSerializerSettings serializerSettings) {
            path = Path.ChangeExtension (path, jsonExtension);

            if (File.Exists (path)) {
                try {
                    string content = File.ReadAllText (path);
                    return JsonConvert.DeserializeObject<T> (content, serializerSettings);
                } catch (Exception exc) {
                    Log.Write (exc);
                }
            }

            return default;
        }

        public static JObject LoadAsJObject (string path) {
            path = Path.ChangeExtension (path, jsonExtension);
            if (File.Exists (path)) {
                return JObject.Parse (File.ReadAllText (path));
            }
            return null;
        }

        public static void SaveJObject (JObject obj, string path) {
            path = Path.ChangeExtension (path, jsonExtension);
            File.WriteAllText (path, obj.ToString ());
        }

        public static void SerializeObject(object obj, string path, bool format = false) {
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

        public static object ConvertObject (object input, Type toType) {
            MethodInfo info = typeof (JSONSerialization).GetMethod ("ConvertObject", new Type[] { typeof (object) }); // Should return the first one in the class, I hope.
            info = info.MakeGenericMethod (toType);
            return info.Invoke (null, new object[] { input });
        }
    }
}
