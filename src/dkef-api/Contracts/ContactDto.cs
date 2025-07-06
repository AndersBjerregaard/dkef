using Ganss.Xss;

namespace Dkef.Contracts;

public class ContactDto : PostObject
{
    public string Email { get; set; } = string.Empty; // 1
    public string FirstName { get; set; } = string.Empty; // 9
    public string LastName { get; set; } = string.Empty; // 10
    public string Title { get; set; } = string.Empty; // 11
    public string Occupation { get; set; } = string.Empty; // 12
    public string WorkTasks { get; set; } = string.Empty; // 13
    public string PrivateAddress { get; set; } = string.Empty; // 14
    public string PrivateZIP { get; set; } = string.Empty; // 15
    public string PrivateCity { get; set; } = string.Empty; // 16
    public string PrivatePhone { get; set; } = string.Empty; // 17
    public string CompanyName { get; set; } = string.Empty; // 18
    public string CompanyAddress { get; set; } = string.Empty; // 19
    public string CompanyZIP { get; set; } = string.Empty; // 20
    public string CompanyCity { get; set; } = string.Empty; // 21
    public string CVRNumber { get; set; } = string.Empty; // 22
    public string CompanyPhone { get; set; } = string.Empty; // 23
    public string CompanyEmail { get; set; } = string.Empty; // 24
    public string ElTeknikDelivery { get; set; } = string.Empty; // 25
    public string EANNumber { get; set; } = string.Empty; // 26
    public string Invoice { get; set; } = string.Empty; // 27
    public string HelpToStudents { get; set; } = string.Empty; // 29
    public string Mentor { get; set; } = string.Empty; // 30
    public string PrimarySection { get; set; } = string.Empty; //31
    public string SecondarySection { get; set; } = string.Empty; // 32
    public string InvoiceEmail { get; set; } = string.Empty; // 33
    public string OldMemberNumber { get; set; } = string.Empty; // 34
    public string ATTInvoice { get; set; } = string.Empty; // 36
    public string ExpectedEndDateOfBeingStudent { get; set; } = string.Empty; // 38

    public override void Sanitize(HtmlSanitizer sanitizer)
    {
        Email = sanitizer.Sanitize(Email);
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
        Invoice = sanitizer.Sanitize(Invoice);
        HelpToStudents = sanitizer.Sanitize(HelpToStudents);
        Mentor = sanitizer.Sanitize(Mentor);
        PrimarySection = sanitizer.Sanitize(PrimarySection);
        SecondarySection = sanitizer.Sanitize(SecondarySection);
        InvoiceEmail = sanitizer.Sanitize(InvoiceEmail);
        OldMemberNumber = sanitizer.Sanitize(OldMemberNumber);
        ATTInvoice = sanitizer.Sanitize(ATTInvoice);
        ExpectedEndDateOfBeingStudent = sanitizer.Sanitize(ExpectedEndDateOfBeingStudent);
    }
}