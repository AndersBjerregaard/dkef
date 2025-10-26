namespace Dkef.Contracts;

public record ResetPasswordDto
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}