using System.Text.Json.Serialization;

namespace Dkef.Contracts.Mailgun;

public sealed class ChangeEmailDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    
    [JsonPropertyName("confirm_link")]
    public string ConfirmLink { get; init; } = string.Empty;
    
    [JsonPropertyName("received_at")]
    public string ReceivedAt { get; init; } = string.Empty;
}