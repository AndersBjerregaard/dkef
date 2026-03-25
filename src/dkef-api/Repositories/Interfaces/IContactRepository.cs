using Dkef.Contracts;
using Dkef.Domain;

namespace Dkef.Repositories;

public interface IContactRepository
{
    Task<ContactListDto> CreateAsync(Contact dto);
    Task<bool> DeleteAsync(Guid id);
    Task<ContactListDto?> GetByIdAsync(Guid id);
    Task<DomainCollection<ContactListDto>> GetMultipleAsync(int take = 10, int skip = 0);
    Task<DomainCollection<ContactListDto>> GetMultipleAsync(IOrderedQueryable<Contact> orderBy, int take = 10, int skip = 0);
    Task<IEnumerable<ContactListDto>> GetMultipleByIdAsync(IEnumerable<Guid> ids);
    Task<ContactListDto> UpdateAsync(Guid id, ContactDto dto);
    Task SeedAsync();
    Task<ContactListDto?> GetByEmailAsync(string email);
    Task<DomainCollection<ContactListDto>> GetMultipleListAsync(int take = 10, int skip = 0);
}
