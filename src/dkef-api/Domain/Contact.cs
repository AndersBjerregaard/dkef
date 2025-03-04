namespace Dkef.Domain;

public class Contact {
    public Guid Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ZIP { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CVRNumber { get; set; } = string.Empty;
    public string EANNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AttributingPerson { get; set; } = string.Empty;
    public string ContactType { get; set; } = string.Empty;
}