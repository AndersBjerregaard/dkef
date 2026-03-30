using Dkef.Attributes;

namespace Dkef.Domain.Abstracts;

/// <summary>
/// Represents a base content entity in the domain.
/// Represents the shared content properties between the content types:
/// <see cref="GeneralAssembly"/>,
/// <see cref="Event"/>,
/// <see cref="News"/>
/// </summary>
public abstract class BaseContent : DomainClass
{
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    [Sortable]
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    [Sortable]
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
}
