namespace Dkef.Contracts;

public record ResetPasswordDto
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}