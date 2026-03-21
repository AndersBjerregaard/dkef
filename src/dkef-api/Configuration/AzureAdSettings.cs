using System.ComponentModel.DataAnnotations;

namespace Dkef.Configuration;

public sealed record AzureAdSettings
{
    [Required]
    public string TenantId { get; init; } = default!;
    [Required]
    public string ClientId { get; init; } = default!;
    [Required]
    public string ClientSecret { get; init; } = default!;
}
