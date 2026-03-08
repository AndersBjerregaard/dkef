using AutoMapper;
using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

// TODO: Consider IdentityUser specific properties when updating
// TODO: Consider foreign key relationship when attempting to delete

public class ContactRepository(ContactContext _context, IMapper _mapper) : IContactRepository
{
    public async Task<Contact> CreateAsync(Contact dto)
    {
        var entityEntry = await _context.Contacts.AddAsync(dto);
        await _context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _context.Contacts.Where(x => x.Id == id.ToString()).ExecuteDeleteAsync() == 1;
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        return await _context.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.ToString());
    }

    public async Task<DomainCollection<Contact>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        var query = _context.Contacts.AsNoTracking()
            .OrderBy(x => x.Id);
        return await GetMultipleAsync(query, take, skip);
    }

    public async Task<DomainCollection<Contact>> GetMultipleAsync(IOrderedQueryable<Contact> orderBy, int take = 10, int skip = 0)
    {
        var totalItems = await _context.Contacts.CountAsync();
        var contacts = await orderBy
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<Contact>(contacts, totalItems);
    }

    public async Task<IEnumerable<Contact>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        return await _context.Contacts.AsNoTracking().Where(x => ids.Contains(Guid.Parse(x.Id))).ToListAsync();
    }

    public async Task<Contact> UpdateAsync(Guid id, ContactDto dto)
    {
        var existing = await _context.Contacts.FirstOrDefaultAsync(x => x.Id == id.ToString())
            ?? throw new KeyNotFoundException($"No contact found with the id {id}");
        var updated = _mapper.Map(dto, existing);
        await _context.SaveChangesAsync();
        return updated;
    }

    public async Task<DomainCollection<ContactListDto>> GetMultipleListAsync(int take = 10, int skip = 0)
    {
        var totalItems = await _context.Contacts.CountAsync();
        var contacts = await _context.Contacts.AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new ContactListDto
            {
                Id = x.Id,
                Email = x.Email ?? string.Empty,
                CreatedAt = x.CreatedAt,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Title = x.Title,
                Occupation = x.Occupation,
                WorkTasks = x.WorkTasks,
                PrivateAddress = x.PrivateAddress,
                PrivateZIP = x.PrivateZIP,
                PrivateCity = x.PrivateCity,
                PrivatePhone = x.PrivatePhone,
                CompanyName = x.CompanyName,
                CompanyAddress = x.CompanyAddress,
                CompanyZIP = x.CompanyZIP,
                CompanyCity = x.CompanyCity,
                CVRNumber = x.CVRNumber,
                CompanyPhone = x.CompanyPhone,
                CompanyEmail = x.CompanyEmail,
                ElTeknikDelivery = x.ElTeknikDelivery,
                EANNumber = x.EANNumber,
                Invoice = x.Invoice,
                HelpToStudents = x.HelpToStudents,
                Mentor = x.Mentor,
                PrimarySection = x.PrimarySection,
                SecondarySection = x.SecondarySection,
                InvoiceEmail = x.InvoiceEmail,
                OldMemberNumber = x.OldMemberNumber,
                RegistrationDate = x.RegistrationDate,
                ATTInvoice = x.ATTInvoice,
                Source = x.Source,
                ExpectedEndDateOfBeingStudent = x.ExpectedEndDateOfBeingStudent,
            })
            .ToListAsync();
        return new DomainCollection<ContactListDto>(contacts, totalItems);
    }

    public async Task SeedAsync() => await ContactContext.SeedAsync(_context);

    public Task<Contact?> GetByEmailAsync(string email)
    {
        return _context.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
    }
}