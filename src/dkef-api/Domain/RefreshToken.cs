using System.ComponentModel.DataAnnotations.Schema;

namespace Dkef.Domain;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public string ContactId { get; set; } = string.Empty;
    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!; // Navigation property
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;
}
