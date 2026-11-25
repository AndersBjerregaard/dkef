namespace Dkef.Domain;

public sealed class SiteImage
{
    public string Group { get; set; } = "";
    public string Slot { get; set; } = "";
    public string ObjectKey { get; set; } = "";
    public string Alt { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
}
