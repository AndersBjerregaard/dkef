using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Ganss.Xss;

namespace Dkef.Contracts;

public sealed class ChangeEmailDto : PostObject {
    [JsonPropertyName("newEmail")]
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        NewEmail = sanitizer.Sanitize(NewEmail);
    }
}