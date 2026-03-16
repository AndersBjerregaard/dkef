using System.ComponentModel.DataAnnotations;

using Ganss.Xss;

namespace Dkef.Contracts;

public sealed class InformationDto : PostObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Phone is required")]
    public string Phone { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Message is required")]
    public string Message { get; set; } = string.Empty;

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Name = sanitizer.Sanitize(Name);
        Phone = sanitizer.Sanitize(Phone);
        Email = sanitizer.Sanitize(Email);
        Message = sanitizer.Sanitize(Message);
    }
}