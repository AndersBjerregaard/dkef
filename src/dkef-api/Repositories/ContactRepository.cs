using AutoMapper;
using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class ContactRepository(ContactContext context, IMapper mapper) : IContactRepository
{
    public async Task<Contact> CreateAsync(Contact dto)
    {
        var entityEntry = await context.Contacts.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await context.Contacts.Where(x => x.Id == id).ExecuteDeleteAsync() == 1;
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        return await context.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Contact>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        return await context.Contacts.AsNoTracking().Skip(skip).Take(take).OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<IEnumerable<Contact>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        return await context.Contacts.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<Contact> UpdateAsync(Guid id, Contact dto)
    {
        var existing = await context.Contacts.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No contact found with the id {id}");
        var updated = mapper.Map(dto, existing);
        await context.SaveChangesAsync();
        return updated;
    }
}