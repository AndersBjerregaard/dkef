using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts;

public class AttachmentCreateDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Filnavn er påkrævet.")]
    public string FileName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "FileId er påkrævet.")]
    public string FileId { get; set; } = string.Empty;

    [Required]
    public long FileSizeBytes { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "MIME-type er påkrævet.")]
    public string MimeType { get; set; } = string.Empty;
}
