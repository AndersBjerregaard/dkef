using Dkef.Domain.Abstracts;
using System.Text.Json.Serialization;

namespace Dkef.Domain;

public class Event : LocatableContent
{
    public override string Kind => "event";
    public DateTime? SignUpDeadline { get; set; }
    [JsonIgnore]
    public ICollection<EventSignUp> SignUps { get; set; } = [];
}
