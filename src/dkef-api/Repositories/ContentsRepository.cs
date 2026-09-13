using Dkef.Data;
using Dkef.Domain;
using Dkef.Domain.Abstracts;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public sealed class ContentRepository(
    ContentsContext context
)
{
    public async Task<TContent> CreateAsync<TContent>(TContent content) where TContent : BaseContent
    {
        context.Add(content);
        await context.SaveChangesAsync();
        return content;
    }

    public async Task<TContent?> GetByIdAsync<TContent>(Guid id) where TContent : BaseContent
        => await context.FindAsync<TContent>(id);

    public async Task<Event?> GetEventByIdWithSignUpsAsync(Guid eventId)
        => await context.Events
            .Include(x => x.SignUps)
            .FirstOrDefaultAsync(x => x.Id == eventId);

    public async Task<bool> IsContactSignedUpForEventAsync(Guid eventId, string contactId)
        => await context.EventSignUps.AnyAsync(x => x.EventId == eventId && x.ContactId == contactId);

    public async Task<EventSignUp> CreateEventSignUpAsync(EventSignUp signUp)
    {
        context.EventSignUps.Add(signUp);
        await context.SaveChangesAsync();
        return signUp;
    }

    public async Task<int> GetEventSignUpCountAsync(Guid eventId)
        => await context.EventSignUps.CountAsync(x => x.EventId == eventId);

    public async Task<IReadOnlyList<EventSignUp>> GetEventSignUpsAsync(Guid eventId)
        => await context.EventSignUps
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.SignedUpAt)
            .ToListAsync();

    public async Task<DomainCollection<BaseContent>> GetMultiple(
        IQueryable<BaseContent> query,
        int take = 10,
        int skip = 0
    )
    {
        var totalItems = await query.CountAsync();
        var contents = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<BaseContent>(contents, totalItems);
    }

    public async Task<DomainCollection<TContent>> GetMultiple<TContent>(
        IQueryable<TContent> query,
        int take = 10,
        int skip = 0
    ) where TContent : BaseContent
    {
        var totalItems = await query.CountAsync();
        var contents = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<TContent>(contents, totalItems);
    }

    public async Task<TContent> UpdateAsync<TContent>(TContent content) where TContent : BaseContent
    {
        context.Update(content);
        await context.SaveChangesAsync();
        return content;
    }

    public async Task DeleteAsync<TContent>(TContent content) where TContent : BaseContent
    {
        context.Remove(content);
        await context.SaveChangesAsync();
    }
}
