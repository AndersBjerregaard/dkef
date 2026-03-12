namespace Dkef.Domain;

public class FeedItem : DomainClass
{
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public System.DateTime CreatedAt { get; set; } = System.DateTime.MinValue;

    // Event + GeneralAssembly specific
    public string? Address { get; set; }
    public System.DateTime? DateTime { get; set; }

    // News specific
    public string? Author { get; set; }
    public System.DateTime? PublishedAt { get; set; }
}
