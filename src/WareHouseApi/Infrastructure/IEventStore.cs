using WareHouseApi.Domain;

namespace WareHouseApi.Infrastructure;

public interface IEventStore
{
    Task SaveEvents(AggregateRoot aggregate, IEnumerable<Event> events, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetEvents(AggregateRoot aggregate, CancellationToken cancellationToken = default);
}
