namespace Dkef.Contracts;

public sealed class EventPaymentReconciliationItemDto
{
    public Guid PaymentRecordId { get; set; }
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string ContactId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public int AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
