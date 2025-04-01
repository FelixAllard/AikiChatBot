using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Utilities.EnumConverter;

public class CommitmentLenghtConverter : JsonConverter<CommitmentLenght>
{
    public override CommitmentLenght Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        
        if (Enum.TryParse<CommitmentLenght>(value, true, out var result)) // 'true' makes it case-insensitive
        {
            return result;
        }
        
        throw new JsonException($"Unable to convert \"{value}\" to {nameof(CommitmentLenght)}.");
    }

    public override void Write(Utf8JsonWriter writer, CommitmentLenght value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
