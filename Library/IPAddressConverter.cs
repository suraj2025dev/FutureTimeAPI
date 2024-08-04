using Newtonsoft.Json;
using System;
using System.Net;

namespace Library {
    public class IPAddressConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IPAddress ipAddress)
            {
                writer.WriteValue(ipAddress.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var ipString = (string)reader.Value;
                if (IPAddress.TryParse(ipString, out var ipAddress))
                {
                    return ipAddress;
                }
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }
    }

}