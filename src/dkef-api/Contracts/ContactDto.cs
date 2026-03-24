using Ganss.Xss;

namespace Dkef.Contracts;

public class ContactDto : PostObject
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ZIP { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyZIP { get; set; } = string.Empty;
    public string CompanyCity { get; set; } = string.Empty;
    public string CVRNumber { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string MagazineDelivery { get; set; } = string.Empty;
    public string EANNumber { get; set; } = string.Empty;
    public string PrimarySection { get; set; } = string.Empty;
    public string SecondarySection { get; set; } = string.Empty;

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Email = sanitizer.Sanitize(Email);
        Name = sanitizer.Sanitize(Name);
        Title = sanitizer.Sanitize(Title);
        EmploymentStatus = sanitizer.Sanitize(EmploymentStatus);
        Address = sanitizer.Sanitize(Address);
        ZIP = sanitizer.Sanitize(ZIP);
        City = sanitizer.Sanitize(City);
        CompanyName = sanitizer.Sanitize(CompanyName);
        CompanyAddress = sanitizer.Sanitize(CompanyAddress);
        CompanyZIP = sanitizer.Sanitize(CompanyZIP);
        CompanyCity = sanitizer.Sanitize(CompanyCity);
        CVRNumber = sanitizer.Sanitize(CVRNumber);
        CompanyPhone = sanitizer.Sanitize(CompanyPhone);
        MagazineDelivery = sanitizer.Sanitize(MagazineDelivery);
        EANNumber = sanitizer.Sanitize(EANNumber);
        PrimarySection = sanitizer.Sanitize(PrimarySection);
        SecondarySection = sanitizer.Sanitize(SecondarySection);
    }
}
