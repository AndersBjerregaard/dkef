
using System.Text.Json.Serialization;

namespace Dkef.Contracts.Mailgun;

public sealed class OldChangeEmailDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("new_email")]
    public string NewEmail { get; init; } = string.Empty;

    [JsonPropertyName("received_at")]
    public string ReceivedAt { get; init; } = string.Empty;

    [JsonPropertyName("revoke_link")]
    public string RevokeLink { get; init; } = string.Empty;
}
