using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Provides methods to manage serialization into Newline Delimited Json format
    /// </summary>
    public static class NdJsonSerializer
    {
        /// <summary>
        /// Deserializes a set of items from nsjson formatted stream.
        /// </summary>
        /// <param name="stream">A readable stream that the json data will be read from.</param>
        /// <param name="jsonSerializerSettings">Settings to use when deserializing items.</param>
        /// <typeparam name="T">Type of the item that will be deserialized into.</typeparam>
        public static async Task<IEnumerable<T>> DeserializeAsync<T>(Stream stream, JsonSerializerSettings jsonSerializerSettings = null) where T : class
        {
            if (null == stream || !stream.CanRead) return Enumerable.Empty<T>();

            JsonSerializer serializer = null == jsonSerializerSettings ? JsonSerializer.CreateDefault() : JsonSerializer.CreateDefault(jsonSerializerSettings);
            serializer.DateParseHandling = DateParseHandling.None;
            StreamReader streamReader = new StreamReader(stream);
            JsonTextReader jsonReader = new JsonTextReader(streamReader)
            {
                SupportMultipleContent = true
            };

            List<T> output = new List<T>();

            while (true)
            {
                try
                {
                    if (!(await jsonReader.ReadAsync())) break;
                }
                catch
                {
                    break;
                }

                T instance = serializer.Deserialize<T>(jsonReader);
                if (null != instance) output.Add(instance);
            }

            return output;
        }
    }
}
