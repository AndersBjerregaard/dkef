using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts;

public record RegisterDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
    public required string ConfirmPassword { get; init; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Primary section is required")]
    public required Dkef.Domain.Section PrimarySection { get; init; }

    public Dkef.Domain.Section? SecondarySection { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string ZIP { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string PrivatePhoneNumber { get; init; } = string.Empty;

    public string EmploymentStatus { get; init; } = string.Empty;

    [Required(ErrorMessage = "Magazine delivery is required")]
    public required string MagazineDelivery { get; init; }

    [Required(ErrorMessage = "Subscription is required")]
    public required string Subscription { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string CompanyAddress { get; init; } = string.Empty;

    public string CompanyZIP { get; init; } = string.Empty;

    public string CompanyCity { get; init; } = string.Empty;

    public string CompanyPhone { get; init; } = string.Empty;

    public string CVRNumber { get; init; } = string.Empty;

    public string EANNumber { get; init; } = string.Empty;
}
