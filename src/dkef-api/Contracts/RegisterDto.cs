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
}
