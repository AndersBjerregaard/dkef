using AutoMapper;

using Dkef.Contracts;
using Dkef.Data;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Repositories;

public class GeneralAssemblyRepository(GeneralAssemblyContext context, IMapper mapper) : IGeneralAssemblyRepository
{
    public async Task<GeneralAssembly> CreateAsync(GeneralAssembly dto)
    {
        var entityEntry = await context.GeneralAssemblies.AddAsync(dto);
        await context.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
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
        var updated = mapper.Map(dto, existing);
        await context.SaveChangesAsync();
        return updated;
    }
}
