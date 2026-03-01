using Dkef.Domain;

namespace Dkef.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(Contact contact);
}
