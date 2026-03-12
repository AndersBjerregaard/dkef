namespace Dkef.Contracts;

/// <summary>
/// Read-only DTO for the paginated members list endpoint.
/// Contains only the Contact-specific domain fields — no IdentityUser properties.
/// </summary>
public class ContactListDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string WorkTasks { get; set; } = string.Empty;
    public string PrivateAddress { get; set; } = string.Empty;
    public string PrivateZIP { get; set; } = string.Empty;
    public string PrivateCity { get; set; } = string.Empty;
    public string PrivatePhone { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyZIP { get; set; } = string.Empty;
    public string CompanyCity { get; set; } = string.Empty;
    public string CVRNumber { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string ElTeknikDelivery { get; set; } = string.Empty;
    public string EANNumber { get; set; } = string.Empty;
    public string Invoice { get; set; } = string.Empty;
    public string HelpToStudents { get; set; } = string.Empty;
    public string Mentor { get; set; } = string.Empty;
    public string PrimarySection { get; set; } = string.Empty;
    public string SecondarySection { get; set; } = string.Empty;
    public string InvoiceEmail { get; set; } = string.Empty;
    public string OldMemberNumber { get; set; } = string.Empty;
    public string RegistrationDate { get; set; } = string.Empty;
    public string ATTInvoice { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ExpectedEndDateOfBeingStudent { get; set; } = string.Empty;
}
