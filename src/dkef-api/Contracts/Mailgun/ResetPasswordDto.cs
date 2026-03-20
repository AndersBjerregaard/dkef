using System.Text.Json.Serialization;

namespace Dkef.Contracts.Mailgun;

public sealed class ResetPasswordDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("change_link")]
    public string ChangeLink { get; init; } = string.Empty;

    [JsonPropertyName("received_at")]
    public string ReceivedAt { get; init; } = string.Empty;
}
