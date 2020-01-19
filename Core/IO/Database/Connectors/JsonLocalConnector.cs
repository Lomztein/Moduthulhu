using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lomztein.Moduthulhu.Core.IO.Database.Connectors
{
    public class JsonLocalConnector : IPathConnector
    {
        private const string LOCAL_FILE_PATH = "/JsonLocalDatabase/";

        private string GetPath()
            => AppContext.BaseDirectory + LOCAL_FILE_PATH;

        public T GetValue<T>(string path)
        {
            Log.Data($"Reading JSON data at '{path}'.");
            var val = JSONSerialization.DeserializeFile<T>(GetPath() + path);
            return val;
        }

        public void SetValue(string path, object value)
        {
            Log.Data($"Storing JSON data {value} at '{path}'.");
            JSONSerialization.SerializeObject(value, GetPath() + path);
        }

        public T[] GetAllValues<T>(string path)
        {
            string directoryPath = GetPath() + path.Substring(0, path.LastIndexOf('/'));
            string prefix = path.Substring(path.LastIndexOf("/") + 1);
            string[] matches = Directory.GetFiles(directoryPath, $"{prefix}*");
            T[] results = new T[matches.Length];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = JSONSerialization.DeserializeFile<T>(matches[i]);
            }
            return results;
        }
    }
}
