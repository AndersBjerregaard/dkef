using Dkef.Data;
using Dkef.Domain.Abstracts;

namespace Dkef.Repositories;

public sealed class ContentsRepository(
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
