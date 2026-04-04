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

    public async Task<DomainCollection<BaseContent>> GetMultiple(
        IOrderedQueryable<BaseContent> orderExpression,
        int take = 10,
        int skip = 0
    )
    {
        var totalItems = await context.Contents.CountAsync();
        var contents = await orderExpression
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return new DomainCollection<BaseContent>(contents, totalItems);
    }

    public async Task<DomainCollection<TContent>> GetMultiple<TContent>(
        IOrderedQueryable<TContent> orderExpression,
        int take = 10,
        int skip = 0
    ) where TContent : BaseContent
    {
        var totalItems = await context.Set<TContent>().CountAsync();
        var contents = await orderExpression
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
