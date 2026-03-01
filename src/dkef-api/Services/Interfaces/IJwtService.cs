using Dkef.Contracts;
using Dkef.Domain;

namespace Dkef.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(Contact contact);
    Task<TokenResponseDto> GenerateTokensAsync(Contact contact);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
