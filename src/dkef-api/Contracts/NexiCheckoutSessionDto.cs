namespace Dkef.Contracts;

public sealed record NexiCheckoutSessionDto
{
    public Guid EventId { get; init; }
    public string PaymentId { get; init; } = string.Empty;
    public int AmountMinor { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string CheckoutKey { get; init; } = string.Empty;
    public string CheckoutJsUrl { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
}
