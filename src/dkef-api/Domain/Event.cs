using Dkef.Domain.Abstracts;

namespace Dkef.Domain;

public class Event : LocatableContent
{
    public override string Kind => "event";
}
