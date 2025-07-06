using Dkef.Contracts;
using Dkef.Domain;

namespace Dkef.Repositories;

public interface IEventsRepository : IRepository<Event, EventDto> {}