namespace Dkef.Contracts;

/// <summary>
/// Read-only DTO for the paginated members list endpoint.
/// Contains only the Contact-specific domain fields — no IdentityUser properties.
/// </summary>
public class ContactListDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ZIP { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CVRNumber { get; set; } = string.Empty;
    public string EANNumber { get; set; } = string.Empty;
    public string PrivatePhoneNumber { get; set; } = string.Empty;
    public string AttPerson { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public uint PaymentDeadlineInDays { get; set; }
    public string TotalSale { get; set; } = string.Empty;
    public string TotalPurchase { get; set; } = string.Empty;
    public DateTime? EnrollmentDate { get; set; }
    public string Subscription { get; set; } = string.Empty;
    public string InvoiceName2 { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyZIP { get; set; } = string.Empty;
    public string CompanyCity { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public Dkef.Domain.Section? PrimarySection { get; set; }
    public Dkef.Domain.Section? SecondarySection { get; set; }
    public string MagazineDelivery { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Dkef.Domain.MemberType MemberType { get; set; }
}
