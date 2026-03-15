using AutoMapper;

using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;
using Dkef.Services;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class GeneralAssemblyRepository(GeneralAssemblyContext context, IMapper mapper, IBucketService bucketService) : IGeneralAssemblyRepository
{
    public async Task<GeneralAssembly> CreateAsync(GeneralAssembly dto)
    {
        var entityEntry = await context.GeneralAssemblies.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Fetch the existing general assembly to get its thumbnail before deletion
        var existing = await context.GeneralAssemblies.FirstOrDefaultAsync(x => x.Id == id);
        if (existing?.ThumbnailUrl != null)
        {
            var imageGuid = ExtractGuidFromUrl(existing.ThumbnailUrl);
            if (imageGuid != Guid.Empty)
            {
                // Fire and forget - don't wait for cleanup, don't block deletion if cleanup fails
                _ = bucketService.DeleteObjectAsync("general-assemblies", imageGuid.ToString()).ConfigureAwait(false);
            }
        }

        return await context.GeneralAssemblies.Where(x => x.Id == id).ExecuteDeleteAsync() == 1;
    }

    public async Task<GeneralAssembly?> GetByIdAsync(Guid id)
    {
        return await context.GeneralAssemblies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<DomainCollection<GeneralAssembly>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        var query = context.GeneralAssemblies.AsNoTracking()
            .OrderBy(x => x.Id);
        return await GetMultipleAsync(query, take, skip);
    }

    public async Task<DomainCollection<GeneralAssembly>> GetMultipleAsync(IOrderedQueryable<GeneralAssembly> orderBy, int take = 10, int skip = 0)
    {
        var totalItems = await context.GeneralAssemblies.CountAsync();
        var assemblies = await orderBy
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<GeneralAssembly>(assemblies, totalItems);
    }

    public async Task<IEnumerable<GeneralAssembly>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        return await context.GeneralAssemblies.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<GeneralAssembly> UpdateAsync(Guid id, GeneralAssemblyDto dto)
    {
        var existing = await context.GeneralAssemblies.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"No general assembly found with the id {id}");

        // If thumbnail is changing, delete the old image from MinIO
        if (existing.ThumbnailUrl != null && Guid.TryParse(dto.ThumbnailId, out var newGuid))
        {
            var oldGuid = ExtractGuidFromUrl(existing.ThumbnailUrl);
            if (oldGuid != Guid.Empty && oldGuid != newGuid)
            {
                // Fire and forget - cleanup failure should not block the update
                _ = bucketService.DeleteObjectAsync("general-assemblies", oldGuid.ToString()).ConfigureAwait(false);
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
