namespace Dkef.Domain;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    public string ThumbnailUrl { get; set; } = string.Empty;
}