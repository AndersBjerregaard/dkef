namespace Dkef.Contracts;

public record ForgotPasswordDto
{
    public required string Email { get; init; }
}