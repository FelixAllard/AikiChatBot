using System.Text.Json;
using System.Text.Json.Serialization;

namespace AikiDataBuilder.Utilities;

public class StringOrArrayConverter : JsonConverter<List<string>>
{
    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return JsonSerializer.Deserialize<List<string>>(ref reader, options) ?? new List<string>();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            return new List<string> { reader.GetString() ?? string.Empty };
        }
        throw new JsonException("Invalid format for Path property.");
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

