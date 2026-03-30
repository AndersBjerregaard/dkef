namespace Dkef.Services;

public interface IAttachmentValidationService
{
    (bool IsValid, string ErrorMessage) ValidateAttachment(string fileName, long fileSizeBytes, string mimeType);
    (bool IsValid, string ErrorMessage) ValidateAttachmentCount(int currentCount, int newCount = 1);
}

public class AttachmentValidationService : IAttachmentValidationService
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB
    private const int MaxAttachmentCount = 3;

    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "text/csv",
        // Additional common document formats
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    };

    public (bool IsValid, string ErrorMessage) ValidateAttachment(string fileName, long fileSizeBytes, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return (false, "Filnavn kan ikke være tomt.");
        }

        if (fileSizeBytes == 0)
        {
            return (false, "Filen kan ikke være tom.");
        }

        if (fileSizeBytes > MaxFileSizeBytes)
        {
            return (false, $"Fil kan maksimalt være 100 MB. Din fil er {FormatBytes(fileSizeBytes)}.");
        }

        if (!AllowedMimeTypes.Contains(mimeType))
        {
            return (false, $"Filtype '{mimeType}' er ikke tilladt. Tilladte typer: PDF, Word, Excel, TXT, CSV, PowerPoint.");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidateAttachmentCount(int currentCount, int newCount = 1)
    {
        if (currentCount + newCount > MaxAttachmentCount)
        {
            return (false, $"Du kan maksimalt vedlægge {MaxAttachmentCount} filer. Du har allerede {currentCount}.");
        }

        return (true, string.Empty);
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
