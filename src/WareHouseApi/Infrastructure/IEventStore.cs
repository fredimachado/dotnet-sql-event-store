using WareHouseApi.Domain;

namespace WareHouseApi.Infrastructure;

public interface IEventStore
{
    Task SaveEventsAsync(AggregateRoot aggregate, IEnumerable<Event> events, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default);
}
