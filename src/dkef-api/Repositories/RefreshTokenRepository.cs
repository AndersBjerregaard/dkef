using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public sealed class RefreshTokenRepository(RefreshTokensContext _context)
{
    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        var entityEntry = await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.Contact)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.Contact)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<RefreshToken>> GetByContactIdAsync(string contactId)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(x => x.ContactId == contactId)
            .ToListAsync();
    }

    public async Task RevokeAsync(Guid id)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No refresh token found with the id {id}");

        if (existing.IsRevoked)
        {
            throw new InvalidOperationException("This refresh token has already been revoked.");
        }

        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task RevokeByTokenAsync(string token)
    {
        var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token)
            ?? throw new KeyNotFoundException($"No refresh token found with the provided token");

        if (existing.IsRevoked)
        {
            throw new InvalidOperationException("This refresh token has already been revoked.");
        }

        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllForContactAsync(string contactId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.ContactId == contactId && !x.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
