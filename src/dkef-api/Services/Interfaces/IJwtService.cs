using Dkef.Domain;

namespace Dkef.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(Contact contact);
}
