using System.Text.Json.Serialization;

namespace Dkef.Contracts.Mailgun;

public sealed class ContactInquiryDto
{
    [JsonPropertyName("sender_name")]
    public string SenderName { get; init; } = string.Empty;

    [JsonPropertyName("sender_email")]
    public string SenderEmail { get; init; } = string.Empty;

    [JsonPropertyName("sender_phone")]
    public string SenderPhone { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
    
    [JsonPropertyName("received_at")]
    public string ReceivedAt { get; init; } = string.Empty;
}