using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dkef.Configuration;
using Dkef.Contracts;
using Dkef.Domain;
using Dkef.Repositories;
using Dkef.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Dkef.Services;

public class JwtService(
    JwtConfig _jwtConfig,
    UserManager<Contact> _userManager,
    RefreshTokenRepository _refreshTokenRepository
) : IJwtService
{
    public async Task<string> GenerateTokenAsync(Contact contact)
    {
        var key = Encoding.UTF8.GetBytes(_jwtConfig.Key);
        var issuer = _jwtConfig.Issuer;
        var audience = _jwtConfig.Audience;
        var expirationMinutes = _jwtConfig.ExpiryMinutes;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, contact.Id),
            new Claim(ClaimTypes.Email, contact.Email!),
            new Claim(JwtRegisteredClaimNames.Sub, contact.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(contact.FirstName))
        {
            claims.Add(new Claim(ClaimTypes.GivenName, contact.FirstName));
        }

        if (!string.IsNullOrEmpty(contact.LastName))
        {
            claims.Add(new Claim(ClaimTypes.Surname, contact.LastName));
        }

        // Add role claims
        var roles = await _userManager.GetRolesAsync(contact);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<TokenResponseDto> GenerateTokensAsync(Contact contact)
    {
        var accessToken = await GenerateTokenAsync(contact);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token in the database
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            ContactId = contact.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Refresh tokens typically last 7 days
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        var expiryInSeconds = _jwtConfig.ExpiryMinutes * 60;

        return new TokenResponseDto(accessToken, refreshToken, expiryInSeconds);
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (!storedToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is no longer active.");
        }

        // Revoke the old refresh token
        await _refreshTokenRepository.RevokeByTokenAsync(refreshToken);

        // Generate new tokens
        return await GenerateTokensAsync(storedToken.Contact);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
