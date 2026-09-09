using System.ComponentModel.DataAnnotations;

namespace Dkef.Configuration;

public sealed record NexiCheckoutConfig
{
    [Required]
    public string ApiBaseUrl { get; init; } = string.Empty;
    [Required]
    public string CheckoutJsUrl { get; init; } = string.Empty;
    [Required]
    public string CheckoutKey { get; init; } = string.Empty;
    [Required]
    public string SecretKey { get; init; } = string.Empty;
    /// <summary>
    /// The URL to the terms and conditions page.
    /// </summary>
    [Required]
    public string TermsUrl { get; init; } = string.Empty;
    /// <summary>
    /// The URL to the privacy and cookie settings page.
    /// </summary>
    [Required]
    public string MerchantTermsUrl { get; init; } = string.Empty;
    [Required]
    public string Language { get; init; } = string.Empty;
    [Required]
    public string Currency { get; init; } = string.Empty;
}
