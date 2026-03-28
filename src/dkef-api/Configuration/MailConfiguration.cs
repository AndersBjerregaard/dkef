using System.ComponentModel.DataAnnotations;

namespace Dkef.Configuration;

public sealed record MailConfiguration
{
    [Required]
    public string Sender { get; init; } = default!;
    [Required]
    public string Domain { get; init; } = default!;
    [Required]
    public string To { get; init; } = default!;
}
