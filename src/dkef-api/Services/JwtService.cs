using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Dkef.Configuration;
using Dkef.Domain;
using Dkef.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Dkef.Services;

public class JwtService(
    JwtConfig _jwtConfig
) : IJwtService
{
    public string GenerateToken(Contact contact)
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
}
