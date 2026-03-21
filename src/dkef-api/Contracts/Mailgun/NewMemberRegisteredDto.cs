using System.Text.Json.Serialization;

namespace Dkef.Contracts.Mailgun;

public sealed class NewMemberRegisteredDto
{
    [JsonPropertyName("full_name")]
    public string FullName { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; init; } = string.Empty;

    [JsonPropertyName("received_at")]
    public string ReceivedAt { get; init; } = string.Empty;
}
