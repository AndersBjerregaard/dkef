using System.ComponentModel.DataAnnotations.Schema;

namespace Dkef.Domain;

public sealed class ForgotPassword
{
    public Guid Id { get; set; }
    public string ContactId { get; set; } = string.Empty;
    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!; // Navigation property
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; } = false;
    [NotMapped]
    public bool IsValid => !IsUsed && (DateTime.UtcNow - RequestedAt).TotalHours < 24;
}
