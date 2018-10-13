using System;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Lomztein.Moduthulhu.Cross
{
    public class Status
    {
        public string Version { get; set; }
        public bool IsRunning { get; set; }

        public string CorePath { get; set; }
        public string ModulesPath { get; set; }
        public string PatcherPath { get; set; }
        public string UpkeeperPath { get; set; }

        public static string GetStatusPath() {
            string currentDirectory = AppContext.BaseDirectory;
            string root = Directory.GetParent (currentDirectory).Parent.FullName;
            return root + "/" + "Status";
        }

        public static T Get<T> (string tokenName) {
            string path = GetStatusPath ();
            JObject status = JSONSerialization.LoadAsJObject (path);
            if (status == null)
                status = JObject.FromObject (new Status ());
            JToken token = status.GetValue (tokenName);
            Log.Write (Log.Type.BOT, $"Getting status token {tokenName}: {token}");
            return token.ToObject<T> ();
        }

        public static void Set (string tokenName, object obj) {
            string path = GetStatusPath ();
            Status status = JSONSerialization.DeserializeFile<Status> (path);
            if (status == null)
                status = new Status ();
            JObject jStatus = JObject.FromObject (status);
            jStatus.GetValue (tokenName).Replace (JToken.FromObject (obj));
            JSONSerialization.SaveJObject (jStatus, GetStatusPath ());
            Log.Write (Log.Type.BOT, $"Setting status token {tokenName}: {obj}");
        }

    }
}
