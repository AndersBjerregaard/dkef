using System.ComponentModel.DataAnnotations.Schema;

namespace Dkef.Domain;

public sealed class ChangeEmail
{
    public Guid Id { get; set; }
    public string ContactId { get; set; } = string.Empty;
    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!; // Navigation property
    public string NewEmail { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset ConfirmedAt { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    [NotMapped]
    public bool IsValid => !IsConfirmed && !IsRevoked && (DateTime.UtcNow - RequestedAt).TotalHours < 24;
}
