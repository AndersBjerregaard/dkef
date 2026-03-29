using System.ComponentModel.DataAnnotations;

using Dkef.Contracts.Validation;

using Ganss.Xss;

namespace Dkef.Contracts;

public class NewsDto : PostObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;
    [GuidValidation(AllowEmpty = true, ErrorMessage = "ThumbnailId must be a valid GUID.")]
    public string ThumbnailId { get; set; } = string.Empty;

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Title = sanitizer.Sanitize(Title);
        Section = sanitizer.Sanitize(Section);
        Author = sanitizer.Sanitize(Author);
        Description = sanitizer.Sanitize(Description);
    }
}
