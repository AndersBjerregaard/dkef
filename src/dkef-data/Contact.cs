using System.Runtime.Serialization;

using Microsoft.AspNetCore.Identity;

namespace Dkef.Data;

public class Contact : IdentityUser
{
    public override string? Email { get; set; } = string.Empty;
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
    public Section? PrimarySection { get; set; }
    public Section? SecondarySection { get; set; }
    public string MagazineDelivery { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public MemberType MemberType { get; set; }
}

public enum MemberType
{
    [EnumMember(Value = "Member")]
    Member,

    [EnumMember(Value = "Board Member")]
    BoardMember,

    [EnumMember(Value = "Admin")]
    Admin
}

public enum Section
{
    [EnumMember(Value = "København Sjælland Sektion")]
    CopenhagenZealand,

    [EnumMember(Value = "Jydsk Sektion")]
    Jutland,

    [EnumMember(Value = "Nordjysk Sektion")]
    NorthJutland,

    [EnumMember(Value = "Sydjysk Sektion")]
    SouthJutland,

    [EnumMember(Value = "Fynsk Sektion")]
    Funen,

    [EnumMember(Value = "Hovedforeningen")]
    MainAssociation
}
