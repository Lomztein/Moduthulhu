using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.Moduthulhu.Core
{
    public static class HTTP
    {
        public static async Task<string> Get(WebRequest req)
        {
            try
            {
                var task = Task.Factory.FromAsync((cb, o) => ((HttpWebRequest)o).BeginGetResponse(cb, o), res => ((HttpWebRequest)res.AsyncState).EndGetResponse(res), req);
                var result = await task;
                var resp = result;
                var stream = resp.GetResponseStream();
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception exc)
            {
                return $"HTTP request failed: {exc.Message}.";
            }
        }

        public static async Task<string> Get(Uri uri)
        {
            HttpWebRequest req = WebRequest.CreateHttp(uri);
            req.AllowReadStreamBuffering = true;
            var tr = await Get(req);
            return tr;
        }

        public static async Task<JObject> GetJSON (Uri uri)
        {
            string response = await Get(uri);
            try
            {
                return JObject.Parse(response);
            } catch
            {
                throw new InvalidOperationException("Unable to parse response. It may not be correct JSON format.");
            }
        }
    }
}
