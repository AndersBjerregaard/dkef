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

    public async Task<DomainCollection<Contact>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        var query = context.Contacts.AsNoTracking()
            .OrderBy(x => x.Id);
        return await GetMultipleAsync(query, take, skip);
    }

    public async Task<DomainCollection<Contact>> GetMultipleAsync(IOrderedQueryable<Contact> orderBy, int take = 10, int skip = 0)
    {
        var totalItems = await context.Contacts.CountAsync();
        var contacts = await orderBy
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<Contact>(contacts, totalItems);
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