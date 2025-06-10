using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LimedikaWebApp.Infrastructure.JsonConverters
{
    public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var instance = JsonSerializer.Deserialize<T>(ref reader, options);
                return new List<T> { instance };
            }

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