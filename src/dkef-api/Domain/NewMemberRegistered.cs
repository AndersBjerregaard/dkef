namespace Dkef.Domain;

public sealed record NewMemberRegistered(
    string FullName,
    string Email,
    Section PrimarySection,
    string Phone = "",
    string Title = "",
    string EmploymentStatus = "",
    string Address = "",
    string ZIP = "",
    string City = "",
    Section? SecondarySection = null,
    string MagazineDelivery = "",
    string Subscription = "",
    string CompanyName = "",
    string CompanyAddress = "",
    string CompanyZIP = "",
    string CompanyCity = "",
    string CompanyPhone = "",
    string CVRNumber = "",
    string EAANNumber = ""
);
