namespace Dkef.Domain;

public class Attachment : DomainClass
{
    public Guid EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.MinValue;
    public string OriginalFileName { get; set; } = string.Empty;
}
