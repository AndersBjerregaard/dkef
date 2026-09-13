namespace Dkef.Domain;

public sealed class EventSignUp
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string ContactId { get; set; } = string.Empty;
    public DateTime SignedUpAt { get; set; } = DateTime.UtcNow;
}
