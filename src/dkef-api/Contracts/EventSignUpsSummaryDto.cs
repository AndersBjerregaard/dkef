namespace Dkef.Contracts;

public sealed class EventSignUpsSummaryDto
{
    public int Total { get; set; }
    public IEnumerable<EventSignUpListItemDto> Collection { get; set; } = [];
}
