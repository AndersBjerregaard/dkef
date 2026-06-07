using System.ComponentModel.DataAnnotations;

using Dkef.Domain;

using Ganss.Xss;

namespace Dkef.Contracts;

public class CreateMemberDto : PostObject
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Section PrimarySection { get; set; }

    // contact details (optional, mapped if provided)
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZIP { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CVRNumber { get; set; } = string.Empty;
    public string EANNumber { get; set; } = string.Empty;
    public string PrivatePhoneNumber { get; set; } = string.Empty;
    public string AttPerson { get; set; } = string.Empty;
    public string Subscription { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyZip { get; set; } = string.Empty;
    public string CompanyCity { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public Section? SecondarySection { get; set; }
    public string MagazineDelivery { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public MemberType MemberType { get; set; } = MemberType.Member;

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Email = sanitizer.Sanitize(Email);
        Name = sanitizer.Sanitize(Name);
        City = sanitizer.Sanitize(City);
        ZIP = sanitizer.Sanitize(ZIP);
        Address = sanitizer.Sanitize(Address);
        CompanyCity = sanitizer.Sanitize(CompanyCity);
        CompanyAddress = sanitizer.Sanitize(CompanyAddress);
        CompanyName = sanitizer.Sanitize(CompanyName);
        CompanyPhone = sanitizer.Sanitize(CompanyPhone);
        EmploymentStatus = sanitizer.Sanitize(EmploymentStatus);
    }

    #if DEBUG
    public bool IsValid(out IReadOnlyList<string> errors)
    {
        var validationContext = new ValidationContext(this);
        var results = new List<ValidationResult>();
        errors = (!Validator.TryValidateObject(this, validationContext, results, true))
            ? results.Select(r => r.ErrorMessage).ToList()!
            : Array.Empty<string>();
        return errors.Count == 0;
    }
    #endif
}