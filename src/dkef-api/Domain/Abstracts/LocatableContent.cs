namespace Dkef.Domain.Abstracts;

public abstract class LocatableContent : BaseContent
{
    public string Address { get; set; } = string.Empty;
}
