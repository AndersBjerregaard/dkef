using AutoMapper;

using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Services;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class NewsRepository(NewsContext context, IMapper mapper, IBucketService bucketService, IAttachmentsRepository attachmentsRepository) : INewsRepository
{
    public async Task<News> CreateAsync(News dto)
    {
        var entityEntry = await context.News.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Delete all attachments for this news item
        await attachmentsRepository.DeleteByEntityIdAsync(id);

        // Fetch the existing news item to get its thumbnail before deletion
        var existing = await context.News.FirstOrDefaultAsync(x => x.Id == id);
        if (existing?.ThumbnailUrl != null)
        {
            var imageGuid = ExtractGuidFromUrl(existing.ThumbnailUrl);
            if (imageGuid != Guid.Empty)
            {
                // Fire and forget - don't wait for cleanup, don't block deletion if cleanup fails
                _ = bucketService.DeleteObjectAsync("news", imageGuid.ToString()).ConfigureAwait(false);
            }
        }

        return await context.News.Where(x => x.Id == id).ExecuteDeleteAsync() == 1;
    }

    public async Task<News?> GetByIdAsync(Guid id)
    {
        return await context.News.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<DomainCollection<News>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        var query = context.News.AsNoTracking()
            .OrderBy(x => x.Id);
        return await GetMultipleAsync(query, take, skip);
    }

    public async Task<DomainCollection<News>> GetMultipleAsync(IOrderedQueryable<News> orderBy, int take = 10, int skip = 0)
    {
        var totalItems = await context.News.CountAsync();
        var news = await orderBy
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<News>(news, totalItems);
    }

    public async Task<IEnumerable<News>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        return await context.News.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<News> UpdateAsync(Guid id, NewsDto dto)
    {
        var existing = await context.News.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No news item found with the id {id}");

        // If thumbnail is changing, delete the old image from MinIO
        if (existing.ThumbnailUrl != null && Guid.TryParse(dto.ThumbnailId, out var newGuid))
        {
            var oldGuid = ExtractGuidFromUrl(existing.ThumbnailUrl);
            if (oldGuid != Guid.Empty && oldGuid != newGuid)
            {
                // Fire and forget - cleanup failure should not block the update
                _ = bucketService.DeleteObjectAsync("news", oldGuid.ToString()).ConfigureAwait(false);
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
