using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Ganss.Xss;

namespace Dkef.Contracts;

public sealed class UpdateProfileDto : PostObject
{
    [JsonPropertyName("name")]
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("employmentStatus")]
    [Required(AllowEmptyStrings = false)]
    public string EmploymentStatus { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("zip")]
    public string ZIP { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("companyAddress")]
    public string CompanyAddress { get; set; } = string.Empty;

    [JsonPropertyName("companyZIP")]
    public string CompanyZIP { get; set; } = string.Empty;

    [JsonPropertyName("companyCity")]
    public string CompanyCity { get; set; } = string.Empty;

    [JsonPropertyName("cvrNumber")]
    public string CVRNumber { get; set; } = string.Empty;

    [JsonPropertyName("companyPhone")]
    public string CompanyPhone { get; set; } = string.Empty;

    [JsonPropertyName("magazineDelivery")]
    public string MagazineDelivery { get; set; } = string.Empty;

    [JsonPropertyName("eanNumber")]
    public string EANNumber { get; set; } = string.Empty;

    [JsonPropertyName("primarySection")]
    [Required]
    public Dkef.Domain.Section PrimarySection { get; set; }

    [JsonPropertyName("secondarySection")]
    public Dkef.Domain.Section? SecondarySection { get; set; }

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Name = sanitizer.Sanitize(Name);
        Title = sanitizer.Sanitize(Title);
        EmploymentStatus = sanitizer.Sanitize(EmploymentStatus);
        Address = sanitizer.Sanitize(Address);
        ZIP = sanitizer.Sanitize(ZIP);
        City = sanitizer.Sanitize(City);
        PhoneNumber = sanitizer.Sanitize(PhoneNumber);
        CompanyName = sanitizer.Sanitize(CompanyName);
        CompanyAddress = sanitizer.Sanitize(CompanyAddress);
        CompanyZIP = sanitizer.Sanitize(CompanyZIP);
        CompanyCity = sanitizer.Sanitize(CompanyCity);
        CVRNumber = sanitizer.Sanitize(CVRNumber);
        CompanyPhone = sanitizer.Sanitize(CompanyPhone);
        MagazineDelivery = sanitizer.Sanitize(MagazineDelivery);
        EANNumber = sanitizer.Sanitize(EANNumber);
    }
}
