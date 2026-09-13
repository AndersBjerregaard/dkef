using System.ComponentModel.DataAnnotations;

using Dkef.Contracts.Validation;

using Ganss.Xss;

namespace Dkef.Contracts;

public class EventDto : PostObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "Section is required.")]
    public string Section { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "Address is required.")]
    public string Address { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "DateTime is required.")]
    [DateTimeValidation(ErrorMessage = "DateTime must be a valid date and time string.")]
    public string DateTime { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;
    [DateTimeValidation(ErrorMessage = "SignUpDeadline must be a valid date and time string.")]
    public string? SignUpDeadline { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "SignUpPriceMinor must be greater than or equal to 0.")]
    public int? SignUpPriceMinor { get; set; }
    [GuidValidation(AllowEmpty = true, ErrorMessage = "ThumbnailId must be a valid GUID.")]
    public string ThumbnailId { get; set; } = string.Empty;
    [GuidArrayValidation(AllowEmpty = true)]
    public string[] AttachmentIds { get; set; } = [];
    public string[] AttachmentFileNames { get; set; } = [];

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Title = sanitizer.Sanitize(Title);
        Section = sanitizer.Sanitize(Section);
        Address = sanitizer.Sanitize(Address);
        Description = sanitizer.Sanitize(Description);
        AttachmentFileNames = AttachmentFileNames.Select(fileName => sanitizer.Sanitize(fileName)).ToArray();
    }
}
