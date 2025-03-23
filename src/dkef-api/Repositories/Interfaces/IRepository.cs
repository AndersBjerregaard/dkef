using Dkef.Domain;

namespace Dkef.Repositories;

public interface IRepository<T> where T : class {
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetMultipleByIdAsync(IEnumerable<Guid> ids);
    Task<DomainCollection<T>> GetMultipleAsync(int take = 10, int skip = 0);
    Task<T> CreateAsync(T dto);
    Task<bool> DeleteAsync(Guid id);
    Task<T> UpdateAsync(Guid id, T dto);
}