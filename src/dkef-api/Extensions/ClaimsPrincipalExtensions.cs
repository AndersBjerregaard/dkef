using System.Security.Claims;

namespace Dkef.Extensions;

public static class ClaimsPrincipalExtensions {
    extension(ClaimsPrincipal source)
    {
        public string? GetEmail() =>
            source.FindFirstValue(ClaimTypes.Email) ?? source.FindFirstValue("email");
        
        public string? GetUserId() =>
            source.FindFirstValue(ClaimTypes.NameIdentifier) ?? source.FindFirstValue("sub");
    }
}