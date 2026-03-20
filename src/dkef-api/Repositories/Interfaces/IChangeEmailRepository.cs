using Dkef.Domain;

namespace Dkef.Repositories;

public interface IChangeEmailRepository
{
    Task<ChangeEmail> CreateAsync(ChangeEmail dto);
    Task<ChangeEmail?> GetByIdAsync(Guid id);
    Task SetAsConfirmedAsync(Guid id);
    Task RevokeAsync(Guid id);
}
