using System.Text.Json;
using System.Text.Json.Serialization;
using Dkef.Domain;

namespace Dkef.Converters;

public class FeedItemKindConverter : JsonConverter<FeedItemKind>
{
    public override FeedItemKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "event" => FeedItemKind.Event,
            "news" => FeedItemKind.News,
            "general-assembly" => FeedItemKind.GeneralAssembly,
            _ => throw new JsonException($"Unknown FeedItemKind value: '{value}'")
        };
    }

    public override void Write(Utf8JsonWriter writer, FeedItemKind value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            FeedItemKind.Event => "event",
            FeedItemKind.News => "news",
            FeedItemKind.GeneralAssembly => "general-assembly",
            _ => throw new JsonException($"Unknown FeedItemKind value: '{value}'")
        };
        writer.WriteStringValue(str);
    }
}
