using System.Text.Json.Serialization;

namespace Dkef.Contracts;

/// <summary>Overrides only. Missing rows use bundled defaults; empty overrides are not resets.</summary>
public sealed record SiteTextGroupResponse(
    [property: JsonPropertyName("group")] string Group,
    [property: JsonPropertyName("overrides")] IReadOnlyDictionary<string, string> Overrides);
