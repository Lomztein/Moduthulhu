using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.ModularDiscordBot.Core.IO
{
    /// <summary>
    /// In many ways just an extension of JSON serialization, however this consistantly saves in a specific folder.
    /// </summary>
    public static class DataSerialization
    {
        public static string dataDirPath = AppContext.BaseDirectory + "Data/";

        public static void SerializeData (object obj, string relativePath) {
            JSONSerialization.SerializeObject (obj, dataDirPath + relativePath);
        }

        public static T DeserializeData<T> (string relativePath) {
            return JSONSerialization.DeserializeFile<T> (dataDirPath + relativePath);
        }
    }
}
