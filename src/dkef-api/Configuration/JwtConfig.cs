namespace Dkef.Configuration;

public sealed record JwtConfig(
    string Key,
    string Issuer,
    string Audience,
    int ExpiryMinutes
);