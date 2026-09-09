namespace Dkef.Contracts;

public sealed record NexiCheckoutSessionDto
{
    public string PaymentId { get; init; } = string.Empty;
    public string CheckoutKey { get; init; } = string.Empty;
    public string CheckoutJsUrl { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
}
