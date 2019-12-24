using System;
using System.Collections.Generic;
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
            return JSONSerialization.DeserializeFile<T>(GetPath() + path);
        }

        public void SetValue(string path, object value)
        {
            Log.Data($"Storing JSON data {value} at '{path}'.");
            JSONSerialization.SerializeObject(value, GetPath() + path);
        }
    }
}
