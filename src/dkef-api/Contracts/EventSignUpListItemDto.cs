namespace Dkef.Contracts;

public sealed class EventSignUpListItemDto
{
    public string ContactId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime SignedUpAt { get; set; }
}
