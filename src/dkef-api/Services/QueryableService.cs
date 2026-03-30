using Dkef.Configuration;

using System.Linq.Dynamic.Core;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Services;

public sealed class QueryableService<TEntity>(
    DbSet<TEntity> table,
    SortablePropertyConfig sortConfig) where TEntity : class
{
    public IOrderedQueryable<TEntity> GetQuery(string orderBy, string order)
    {
        if (!string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Can only order by ascending or descending");
        }
        if (!sortConfig.IsPropertySortable<TEntity>(orderBy))
        {
            throw new InvalidOperationException($"Type {typeof(TEntity).Name} does not support ordering by {orderBy}");
        }
        IQueryable<TEntity> query = table.AsNoTracking();
        return query.OrderBy($"{orderBy} {order}");
    }
}
