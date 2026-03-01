using System.ComponentModel.DataAnnotations;

namespace Dkef.Contracts;

public record RegisterDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
    public required string ConfirmPassword { get; init; }

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
