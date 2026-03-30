namespace Dkef.Contracts;

public class AttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
}
