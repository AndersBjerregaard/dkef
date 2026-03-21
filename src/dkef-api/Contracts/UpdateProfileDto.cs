using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Ganss.Xss;

namespace Dkef.Contracts;

public sealed class UpdateProfileDto : PostObject
{
    [JsonPropertyName("firstName")]
    [Required(AllowEmptyStrings = false)]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    [Required(AllowEmptyStrings = false)]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("occupation")]
    [Required(AllowEmptyStrings = false)]
    public string Occupation { get; set; } = string.Empty;

    [JsonPropertyName("workTasks")]
    public string WorkTasks { get; set; } = string.Empty;

    [JsonPropertyName("privateAddress")]
    public string PrivateAddress { get; set; } = string.Empty;

    [JsonPropertyName("privateZIP")]
    public string PrivateZIP { get; set; } = string.Empty;

    [JsonPropertyName("privateCity")]
    public string PrivateCity { get; set; } = string.Empty;

    [JsonPropertyName("privatePhone")]
    public string PrivatePhone { get; set; } = string.Empty;

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

    [JsonPropertyName("companyEmail")]
    public string CompanyEmail { get; set; } = string.Empty;

    [JsonPropertyName("elTeknikDelivery")]
    public string ElTeknikDelivery { get; set; } = string.Empty;

    [JsonPropertyName("eanNumber")]
    public string EANNumber { get; set; } = string.Empty;

    [JsonPropertyName("primarySection")]
    [Required(AllowEmptyStrings = false)]
    public string PrimarySection { get; set; } = string.Empty;

    [JsonPropertyName("secondarySection")]
    public string SecondarySection { get; set; } = string.Empty;

    [JsonPropertyName("invoiceEmail")]
    public string InvoiceEmail { get; set; } = string.Empty;

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        FirstName = sanitizer.Sanitize(FirstName);
        LastName = sanitizer.Sanitize(LastName);
        Title = sanitizer.Sanitize(Title);
        Occupation = sanitizer.Sanitize(Occupation);
        WorkTasks = sanitizer.Sanitize(WorkTasks);
        PrivateAddress = sanitizer.Sanitize(PrivateAddress);
        PrivateZIP = sanitizer.Sanitize(PrivateZIP);
        PrivateCity = sanitizer.Sanitize(PrivateCity);
        PrivatePhone = sanitizer.Sanitize(PrivatePhone);
        CompanyName = sanitizer.Sanitize(CompanyName);
        CompanyAddress = sanitizer.Sanitize(CompanyAddress);
        CompanyZIP = sanitizer.Sanitize(CompanyZIP);
        CompanyCity = sanitizer.Sanitize(CompanyCity);
        CVRNumber = sanitizer.Sanitize(CVRNumber);
        CompanyPhone = sanitizer.Sanitize(CompanyPhone);
        CompanyEmail = sanitizer.Sanitize(CompanyEmail);
        ElTeknikDelivery = sanitizer.Sanitize(ElTeknikDelivery);
        EANNumber = sanitizer.Sanitize(EANNumber);
        PrimarySection = sanitizer.Sanitize(PrimarySection);
        SecondarySection = sanitizer.Sanitize(SecondarySection);
        InvoiceEmail = sanitizer.Sanitize(InvoiceEmail);
    }
}
