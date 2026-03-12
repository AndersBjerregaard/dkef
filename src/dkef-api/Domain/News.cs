using Dkef.Attributes;

namespace Dkef.Domain;

public class News : DomainClass
{
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    [Sortable]
    public DateTime PublishedAt { get; set; } = DateTime.MinValue;
    [Sortable]
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
}
