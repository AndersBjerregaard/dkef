using Dkef.Attributes;

namespace Dkef.Domain;

public class Event : DomainClass
{
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    [Sortable]
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    [Sortable]
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
}