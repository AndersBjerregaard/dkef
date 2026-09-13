namespace Dkef.Domain;

public sealed class EventSignUpPayment
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string ContactId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public int AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
