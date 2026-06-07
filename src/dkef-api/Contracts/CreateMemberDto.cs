using System.ComponentModel.DataAnnotations;

using Dkef.Domain;

using Ganss.Xss;

namespace Dkef.Contracts;

public class CreateMemberDto : PostObject
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Section PrimarySection { get; set; }

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Email = sanitizer.Sanitize(Email);
        Name = sanitizer.Sanitize(Name);
    }
}
