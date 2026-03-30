using AutoMapper;

using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Services;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class EventsRepository(EventsContext context, IMapper mapper, IBucketService bucketService, IAttachmentsRepository attachmentsRepository) : IEventsRepository
{
    public async Task<Event> CreateAsync(Event dto)
    {
        var entityEntry = await context.Events.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Delete all attachments for this event
        await attachmentsRepository.DeleteByEntityIdAsync(id);

        // Fetch the existing event to get its thumbnail before deletion
        var existing = await context.Events.FirstOrDefaultAsync(x => x.Id == id);
        if (existing?.ThumbnailUrl != null)
        {
            var imageGuid = ExtractGuidFromUrl(existing.ThumbnailUrl);
            if (imageGuid != Guid.Empty)
            {
                // Fire and forget - don't wait for cleanup, don't block deletion if cleanup fails
                _ = bucketService.DeleteObjectAsync("events", imageGuid.ToString()).ConfigureAwait(false);
            }
        }

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
        return await GetMultipleAsync(query, take, skip);
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

    public async Task<Event> UpdateAsync(Guid id, EventDto dto)
    {
        var existing = await context.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No event found with the id {id}");

        // If thumbnail is changing, delete the old image from MinIO
        if (existing.ThumbnailUrl != null && Guid.TryParse(dto.ThumbnailId, out var newGuid))
        {
            var oldGuid = ExtractGuidFromUrl(existing.ThumbnailUrl);
            if (oldGuid != Guid.Empty && oldGuid != newGuid)
            {
                // Fire and forget - cleanup failure should not block the update
                _ = bucketService.DeleteObjectAsync("events", oldGuid.ToString()).ConfigureAwait(false);
            }
        }

        var updated = mapper.Map(dto, existing);
        await context.SaveChangesAsync();
        return updated;
    }

    private static Guid ExtractGuidFromUrl(string url)
    {
        try
        {
            var parts = url.Split('/');
            var lastPart = parts[^1];
            return Guid.TryParse(lastPart, out var guid) ? guid : Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }
}