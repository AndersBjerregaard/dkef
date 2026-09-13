namespace Dkef.Contracts;

public sealed class EventSignUpCreateResponseDto
{
    public Guid EventId { get; set; }
    public string ContactId { get; set; } = string.Empty;
    public DateTime SignedUpAt { get; set; }
}
