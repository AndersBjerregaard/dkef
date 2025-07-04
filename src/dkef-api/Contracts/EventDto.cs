using System.ComponentModel.DataAnnotations;
using Dkef.Contracts.Validation;

namespace Dkef.Contracts;

public class EventDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Section is required.")]
    public string Section { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "Address is required.")]
    public string Address { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "DateTime is required.")]
    [DateTimeValidation(ErrorMessage = "DateTime must be a valid date and time string.")]
    public string DateTime { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "ThumbnailId is required.")]
    [GuidValidation(ErrorMessage = "ThumbnailId must be a valid GUID.")]
    public string ThumbnailId { get; set; } = string.Empty;
}