using DataFac.Memory;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DTOMaker.Runtime.JsonSystemText
{
    public class PairOfInt16Converter : JsonConverter<PairOfInt16>
    {
        public override PairOfInt16 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject token");

            Int16 a = default;
            Int16 b = default;

            // get pair of values
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName token");

                string propertyName = reader.GetString()!;
                reader.Read();
                Int16 value = reader.GetInt16();
                switch (propertyName)
                {
                    case "A":
                    case "a":
                        a = value;
                        break;
                    case "B":
                    case "c":
                        b = value;
                        break;
                    default:
                        throw new JsonException($"Unexpected property name: {propertyName}");
                }
            }

            return new PairOfInt16(a, b);
        }

        public override void Write(Utf8JsonWriter writer, PairOfInt16 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("A", value.A);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }
    }

    public class PairOfInt32Converter : JsonConverter<PairOfInt32>
    {
        public override PairOfInt32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject token");

            Int32 a = default;
            Int32 b = default;

            // get pair of values
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName token");

                string propertyName = reader.GetString()!;
                reader.Read();
                Int32 value = reader.GetInt32();
                switch (propertyName)
                {
                    case "A":
                    case "a":
                        a = value;
                        break;
                    case "B":
                    case "c":
                        b = value;
                        break;
                    default:
                        throw new JsonException($"Unexpected property name: {propertyName}");
                }
            }

            return new PairOfInt32(a, b);
        }

        public override void Write(Utf8JsonWriter writer, PairOfInt32 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("A", value.A);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }
    }

    public class PairOfInt64Converter : JsonConverter<PairOfInt64>
    {
        public override PairOfInt64 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject token");

            long a = default;
            long b = default;

            // get pair of values
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName token");

                string propertyName = reader.GetString()!;
                reader.Read();
                Int64 value = reader.GetInt64();
                switch (propertyName)
                {
                    case "A":
                    case "a":
                        a = value;
                        break;
                    case "B":
                    case "c":
                        b = value;
                        break;
                    default:
                        throw new JsonException($"Unexpected property name: {propertyName}");
                }
            }

            return new PairOfInt64(a, b);
        }

        public override void Write(Utf8JsonWriter writer, PairOfInt64 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("A", value.A);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }
    }

    public static class SerializationHelpers
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
            {
                new PairOfInt16Converter(),
                new PairOfInt32Converter(),
                new PairOfInt64Converter(),
            }
        };

        public static string SerializeToJson<T>(this T value)
        {
            return JsonSerializer.Serialize<T>(value, _options);
        }

        public static T? DeserializeFromJson<T>(this string input)
        {
            return JsonSerializer.Deserialize<T>(input, _options);
        }
    }
}
