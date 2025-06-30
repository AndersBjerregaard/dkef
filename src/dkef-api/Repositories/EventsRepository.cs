
using AutoMapper;

using Dkef.Data;
using Dkef.Domain;

namespace Dkef.Repositories;

public class EventsRepository(EventsContext context, IMapper mapper) : IEventsRepository
{
    public Task<Event> CreateAsync(Event dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Event?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<DomainCollection<Event>> GetMultipleAsync(int take = 10, int skip = 0)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Event>> GetMultipleByIdAsync(IEnumerable<Guid> ids)
    {
        throw new NotImplementedException();
    }

    public Task<Event> UpdateAsync(Guid id, Event dto)
    {
        throw new NotImplementedException();
    }
}