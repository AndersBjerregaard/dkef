using Dkef.Contracts;
using Dkef.Domain;

namespace Dkef.Repositories;

public interface IContactRepository : IRepository<Contact, ContactDto>
{
    Task SeedAsync();
    Task<Contact?> GetByEmailAsync(string email);
    Task<DomainCollection<ContactListDto>> GetMultipleListAsync(int take = 10, int skip = 0);
}