namespace Dkef.Contracts.Nexi;

// API source: https://developer.nexigroup.com/nexi-checkout/en-EU/api/payment-v1/#v1-payments-post

public sealed record NexiCreatePaymentRequest
{
    public required NexiOrder Order { get; init; }
    public required NexiCheckout Checkout { get; init; }
}

public sealed record NexiOrder
{
    public required IReadOnlyList<NexiOrderItem> Items { get; init; }
    public required int Amount { get; init; }
    public required string Currency { get; init; }
    public string? Reference { get; init; }
}

public sealed record NexiOrderItem
{
    public required string Reference { get; init; }
    public required string Name { get; init; }
    public required double Quantity { get; init; }
    public required string Unit { get; init; }
    public required int UnitPrice { get; init; }
    public int TaxRate { get; init; }
    public int TaxAmount { get; init; }
    public required int GrossTotalAmount { get; init; }
    public required int NetTotalAmount { get; init; }
}

public sealed record NexiCheckout
{
    public required string IntegrationType { get; init; }
    public required string Url { get; init; }
    public required string TermsUrl { get; init; }
    public required string MerchantTermsUrl { get; init; }
    public bool Charge { get; init; }
}
