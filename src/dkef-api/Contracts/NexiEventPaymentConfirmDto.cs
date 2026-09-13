using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts;

public sealed record NexiEventPaymentConfirmDto
{
    [Required]
    public string PaymentId { get; init; } = string.Empty;
}
