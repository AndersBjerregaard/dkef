using AutoMapper;

using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class NewsRepository(NewsContext context, IMapper mapper) : INewsRepository
{
    public async Task<News> CreateAsync(News dto)
    {
        var entityEntry = await context.News.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
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
        var updated = mapper.Map(dto, existing);
        await context.SaveChangesAsync();
        return updated;
    }
}
