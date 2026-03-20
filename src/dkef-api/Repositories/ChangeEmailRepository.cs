using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public sealed class ChangeEmailRepository(ChangeEmailContext _context) : IChangeEmailRepository
{
    public async Task<ChangeEmail> CreateAsync(ChangeEmail dto)
    {
        var entityEntry = await _context.ChangeEmails.AddAsync(dto);
        await _context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<ChangeEmail?> GetByIdAsync(Guid id) =>
        await _context.ChangeEmails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task SetAsConfirmedAsync(Guid id)
    {
        var existing = await _context.ChangeEmails.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No forgot password request found with the id {id}");
        if (!existing.IsValid)
        {
            throw new InvalidOperationException("Cannot mark an already used or expired forgot password request as used.");
        }
        existing.IsConfirmed = true;
        existing.ConfirmedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAsync(Guid id)
    {
        var existing = await _context.ChangeEmails.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No forgot password request found with the id {id}");
        existing.IsRevoked = true;
        await _context.SaveChangesAsync();
    }
}
