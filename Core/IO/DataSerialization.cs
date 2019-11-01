using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO
{
    /// <summary>
    /// In many ways just an extension of JSON serialization, however this consistantly saves in a specific folder.
    /// </summary>
    public static class DataSerialization
    {
        public static string DataPath => AppContext.BaseDirectory + "Data/";

        public static void SerializeData (object obj, string relativePath) {
            JSONSerialization.SerializeObject (obj, DataPath + relativePath);
        }

        public static T DeserializeData<T> (string relativePath) {
            return DeserializeData<T> (relativePath, null);
        }

        public static T DeserializeData<T>(string relativePath, JsonSerializerSettings settings) {
            return JSONSerialization.DeserializeFile<T> (DataPath + relativePath, settings);
        }
    }
}
