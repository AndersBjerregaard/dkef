using Dkef.Configuration;
using Dkef.Domain;
using System.Linq.Dynamic.Core;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Services;

public sealed class QueryableService<TEntity>(DbSet<TEntity> _table, SortablePropertyConfig _sortConfig) where TEntity : DomainClass
{
    public IOrderedQueryable<TEntity> GetQuery(string orderBy, string order)
    {
        if (!string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Can only order by ascending or descending");
        }
        if (!_sortConfig.IsPropertySortable<TEntity>(orderBy))
        {
            throw new InvalidOperationException($"Type {typeof(TEntity).Name} does not support ordering by {orderBy}");
        }
        IQueryable<TEntity> query = _table.AsNoTracking();
        return query.OrderBy($"{orderBy} {order}");
    }
}