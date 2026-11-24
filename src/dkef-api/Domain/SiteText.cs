namespace Dkef.Domain;

/// <summary>A published plain-text override, independent of the content feed.</summary>
public sealed class SiteText
{
    public string Group { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    // Retained audit data, deliberately not a cross-context Contact relationship.
    public string UpdatedBy { get; set; } = string.Empty;
}
