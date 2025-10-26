using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public sealed class ForgotPasswordRepository(ForgotPasswordContext _context)
{
    public async Task<ForgotPassword> CreateAsync(ForgotPassword dto)
    {
        var entityEntry = await _context.ForgotPasswords.AddAsync(dto);
        await _context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<ForgotPassword?> GetByIdAsync(Guid id)
    {
        return await _context.ForgotPasswords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task SetAsUsedAsync(Guid id)
    {
        var existing = await _context.ForgotPasswords.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No forgot password request found with the id {id}");
        if (!existing.IsValid)
        {
            throw new InvalidOperationException("Cannot mark an already used or expired forgot password request as used.");
        }
        existing.IsUsed = true;
        await _context.SaveChangesAsync();
    }
}