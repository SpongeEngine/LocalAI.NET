using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LocalAI.NET.Utils
{
    /// <summary>
    /// Provides JSON serialization utilities
    /// </summary>
    public static class JsonSerializer
    {
        private static readonly JsonSerializerSettings DefaultSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        /// <summary>
        /// Serializes an object to JSON
        /// </summary>
        public static string Serialize(object value, JsonSerializerSettings? settings = null)
        {
            return JsonConvert.SerializeObject(value, settings ?? DefaultSettings);
        }

        /// <summary>
        /// Deserializes JSON to an object
        /// </summary>
        public static T? Deserialize<T>(string json, JsonSerializerSettings? settings = null)
        {
            return JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);
        }
    }
}