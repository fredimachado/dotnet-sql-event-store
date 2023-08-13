using WareHouseApi.Domain;
using WareHouseApi.Infrastructure;

namespace Warehouse.Tests.Infrastructure;

public class InMemoryEventStore : IEventStore
{
    public List<Event> Events { get; } = new();

    public Task<IEnumerable<Event>> GetEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Events.AsEnumerable());
    }

    public Task SaveEventsAsync(AggregateRoot aggregate, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        Events.AddRange(events);

        return Task.CompletedTask;
    }
}
