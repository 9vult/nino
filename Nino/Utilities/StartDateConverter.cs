using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nino.Utilities
{
    internal class StartDateConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Read the object { "year": 2024, "month": 7, "day": 7 }
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            int year = 0, month = 0, day = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "year":
                            year = ReadNullableInt(ref reader) ?? 0;
                            break;
                        case "month":
                            month = ReadNullableInt(ref reader) ?? 1;
                            break;
                        case "day":
                            day = ReadNullableInt(ref reader) ?? 1;
                            break;
                    }
                }
            }

            return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("year", value.Year);
            writer.WriteNumber("month", value.Month);
            writer.WriteNumber("day", value.Day);
            writer.WriteEndObject();
        }
        
        private int? ReadNullableInt(ref Utf8JsonReader reader)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.Number when reader.TryGetInt32(out var value) => value,
                _ => throw new JsonException("Expected a null or integer value.")
            };
        }

    }
}
