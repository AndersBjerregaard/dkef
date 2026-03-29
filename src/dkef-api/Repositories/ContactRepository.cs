using AutoMapper;

using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class ContactRepository(ContactsContext _context, IMapper _mapper) : IContactRepository
{
    public async Task<ContactListDto> CreateAsync(Contact dto)
    {
        var entityEntry = await _context.Contacts.AddAsync(dto);
        await _context.SaveChangesAsync();
        var newEntry = entityEntry.Entity;
        return ContactListDto.FromContact(_mapper, newEntry);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _context.Contacts.Where(x => x.Id == id.ToString()).ExecuteDeleteAsync() == 1;
    }

    public async Task<ContactListDto?> GetByIdAsync(Guid id)
    {
        var hit = await _context.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.ToString());
        return hit is null ? null! : ContactListDto.FromContact(_mapper, hit);
    }

    public async Task<DomainCollection<ContactListDto>> GetMultipleAsync(uint take = 10, uint skip = 0)
    {
        var query = _context.Contacts.AsNoTracking()
            .OrderBy(x => x.Id);
        return await GetMultipleAsync(query, take, skip);
    }

    public async Task<DomainCollection<ContactListDto>> GetMultipleAsync(IOrderedQueryable<Contact> orderBy, uint take = 10, uint skip = 0)
    {
        var totalItems = await _context.Contacts.CountAsync();
        var contacts = await orderBy
            .Skip((int)skip)
            .Take((int)take)
            .Select(x => ContactListDto.FromContact(_mapper, x))
            .ToListAsync();
        return new DomainCollection<ContactListDto>(contacts, totalItems);
    }

    public async Task<DomainCollection<ContactListDto>> GetMultipleAsync(
        IQueryable<Contact> query,
        uint take = 10,
        uint skip = 0)
    {
        var totalItems = await query.CountAsync();
        var contacts = await query
            .OrderBy(x => x.Id)
            .Skip((int)skip)
            .Take((int)take)
            .Select(x => ContactListDto.FromContact(_mapper, x))
            .ToListAsync();
        return new DomainCollection<ContactListDto>(contacts, totalItems);
    }

    public async Task<IEnumerable<ContactListDto>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        var items = await _context.Contacts.AsNoTracking().Where(x => ids.Contains(Guid.Parse(x.Id))).ToListAsync();
        return items.Select(x => ContactListDto.FromContact(_mapper, x));
    }

    public async Task<ContactListDto> UpdateAsync(Guid id, ContactDto dto)
    {
        var existing = await _context.Contacts.FirstOrDefaultAsync(x => x.Id == id.ToString())
            ?? throw new KeyNotFoundException($"No contact found with the id {id}");
        var updated = _mapper.Map(dto, existing);
        await _context.SaveChangesAsync();
        var dtoUpdated = ContactListDto.FromContact(_mapper, updated);
        return dtoUpdated;
    }

    public async Task<DomainCollection<ContactListDto>> GetMultipleListAsync(
        uint take = 10,
        uint skip = 0,
        MemberType? memberType = null)
    {
        if (memberType is not null)
        {
            var query = _context.Contacts.AsNoTracking().Where(x => x.MemberType == memberType.Value);
            return await GetMultipleAsync(query, take, skip);
        }
        var totalItems = await _context.Contacts.CountAsync();
        var contacts = await _context.Contacts.AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip((int)skip)
            .Take((int)take)
            .Select(x => ContactListDto.FromContact(_mapper, x))
            .ToListAsync();
        return new DomainCollection<ContactListDto>(contacts, totalItems);
    }

    public async Task SeedAsync() => await ContactsContext.SeedAsync(_context);

    public async Task<ContactListDto?> GetByEmailAsync(string email)
    {
        var hit = await _context.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
        return hit is null ? null! : ContactListDto.FromContact(_mapper, hit);
    }
}
