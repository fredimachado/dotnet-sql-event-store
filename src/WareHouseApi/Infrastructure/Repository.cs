using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WareHouseApi.Domain;

namespace WareHouseApi.Infrastructure;

public class Repository<TAggregate> where TAggregate : AggregateRoot
{
    private readonly IEventStore _eventStore;

    public Repository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        await _eventStore.SaveEventsAsync(aggregate, aggregate.GetPendingEvents(), cancellationToken);
    }

    public async Task<TAggregate> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), id);

        var events = await _eventStore.GetEventsAsync(aggregate, cancellationToken);
        
        aggregate.ApplyEvents(events);

        return aggregate;
    }
}
