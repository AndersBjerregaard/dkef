using Dkef.Contracts;
using Dkef.Domain;

namespace Dkef.Repositories;

public interface IContactRepository : IRepository<Contact, ContactDto>
{
    Task SeedAsync();
}