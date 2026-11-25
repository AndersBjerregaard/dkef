using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dkef.Services;

public static class SiteTextAuditIdentity
{
    /// <summary>
    /// JwtService emits Identity ID as nameid and email as sub. Inbound mapping maps BOTH to
    /// NameIdentifier, retaining the original name in claim properties. Never use FindFirst here.
    /// </summary>
    public static string? FindIdentityId(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        var ids = principal.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier
            && claim.Properties.TryGetValue(JwtSecurityTokenHandler.ShortClaimTypeProperty, out var original)
            && original == "nameid").Select(claim => claim.Value).ToArray();
        return ids.Length == 1 && !string.IsNullOrWhiteSpace(ids[0]) ? ids[0] : null;
    }
}
