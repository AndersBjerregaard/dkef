using System.Collections.Frozen;
using System.Text.Json.Serialization;

using Dkef.Services;

using Ganss.Xss;

namespace Dkef.Contracts;

/// <summary>
/// Construct only through SiteTextPatchContract, which checks the raw JSON before dictionaries can
/// hide duplicate properties. Not a model-binding DTO: endpoint integration belongs to item 04.
/// </summary>
public sealed class SiteTextPatchRequest : PostObject
{
    [JsonPropertyName("changes")]
    public FrozenDictionary<string, string> Changes { get; private set; }

    internal SiteTextPatchRequest(Dictionary<string, string> changes) =>
        Changes = changes.ToFrozenDictionary(StringComparer.Ordinal);

    public override void Sanitize(HtmlSanitizer sanitizer) =>
        Changes = Changes.ToFrozenDictionary(pair => pair.Key,
            pair => SiteTextValueRules.Sanitize(pair.Value, sanitizer), StringComparer.Ordinal);
}
