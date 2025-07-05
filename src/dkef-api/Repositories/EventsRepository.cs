using AutoMapper;
using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class EventsRepository(EventsContext context, IMapper mapper) : IEventsRepository
{
    public async Task<Event> CreateAsync(Event dto)
    {
        var entityEntry = await context.Events.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await context.Events.Where(x => x.Id == id).ExecuteDeleteAsync() == 1;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await context.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<DomainCollection<Event>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        var query = context.Events.AsNoTracking()
            .OrderBy(x => x.Id);
        return await GetMultipleAsync(query, skip, take);
    }

    public async Task<DomainCollection<Event>> GetMultipleAsync(IOrderedQueryable<Event> orderBy, int take = 10, int skip = 0)
    {
        var totalItems = await context.Events.CountAsync();
        var events = await orderBy
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<Event>(events, totalItems);
    }

    public async Task<IEnumerable<Event>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        return await context.Events.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<Event> UpdateAsync(Guid id, Event dto)
    {
        var existing = await context.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No event found with the id {id}");
        var updated = mapper.Map(dto, existing);
        await context.SaveChangesAsync();
        return updated;
    }
}