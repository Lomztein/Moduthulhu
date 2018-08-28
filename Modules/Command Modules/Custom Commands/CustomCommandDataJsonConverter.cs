/*using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

//TODO: Figure out loading of the nested base class of CustomChainData and CustomSetData, allowing for infinitely nested custom commands.

namespace Lomztein.Moduthulhu.Modules.CustomCommands.Data {

    public class CustomCommandDataJsonConverter : JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return (objectType == typeof (CustomSetData) || objectType == (typeof (CustomChainData)) || objectType == (typeof (CustomCommandData)));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            JsonSerializer emptySerializer = new JsonSerializer ();

            // It is a "root" custom set.
            if (objectType == typeof (CustomSetData))
                return emptySerializer.Deserialize<CustomSetData> (reader);

            // It is a nested command, be it set or not.
            if (objectType == typeof (CustomCommandData)) {

                var value = reader.Value;

            }

            return null;
        }

        // Writing works just fine as it is, it is reading we need to worry about.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException ();
        }
    }

    public class CustomCommandSetJsonConverter : JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return (objectType == typeof (CustomSetData) || objectType == (typeof (CustomChainData)) || objectType == (typeof (CustomCommandData)));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            JsonSerializer dataSerializer = new JsonSerializer () { Converters = { new CustomCommandDataJsonConverter () } };

            // It is a "root" custom set.
            if (objectType == typeof (CustomSetData))
                return dataSerializer.Deserialize<CustomSetData> (reader);

            return null;
        }

        // Writing works just fine as it is, it is reading we need to worry about.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException ();
        }
    }
}*/