using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace LimedikaWebApp.Infrastructure.JsonConverters
{
    /// <summary>
    /// Converter for handling JSON properties that can be either a single object or an array of objects.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list</typeparam>
    public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
    {
        /// <summary>
        /// Reads JSON and converts it to a List<T>.
        /// </summary>
        /// <param name="reader">Utf8JsonReader to read from.</param>
        /// <param name="typeToConvert">Type of the object to convert</param>
        /// <param name="options">Serializer options</param>
        /// <returns>A List<T> containing the deserialized objects</returns>
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // JSON value is a single object
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var instance = JsonSerializer.Deserialize<T>(ref reader, options);
                return new List<T> { instance };
            }
            //JSON value is an array of objects
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return JsonSerializer.Deserialize<List<T>>(ref reader, options);
            }

            // Handle cases like "data": "" or "data": null
            if (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.Null)
            {
                reader.Skip(); // Move past the value
                return new List<T>();
            }

            throw new JsonException($"Unexpected JSON token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            // The default writer is fine, no need for custom write logic
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}