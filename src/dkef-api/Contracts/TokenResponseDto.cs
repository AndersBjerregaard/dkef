namespace Dkef.Contracts;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);
