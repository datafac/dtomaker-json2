using System.Text.Json;
using System.Text.Json.Serialization;

namespace DTOMaker.Runtime.JsonSystemText
{
    public static class SerializationHelpers
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
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
