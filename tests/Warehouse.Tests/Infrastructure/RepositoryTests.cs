using WareHouseApi.Domain;
using WareHouseApi.Domain.Entities;
using WareHouseApi.Domain.Events;
using WareHouseApi.Infrastructure;

namespace Warehouse.Tests.Infrastructure;

public class RepositoryTests
{
    private readonly Repository<WarehouseProduct> _sut;
    private readonly InMemoryEventStore _inMemoryEventStore = new();

    public RepositoryTests()
    {
        _sut = new Repository<WarehouseProduct>(_inMemoryEventStore);
    }

    [Fact]
    public async Task GetById()
    {
        var id = Guid.NewGuid();
        _inMemoryEventStore.Events.AddRange(new Event[]
        {
            new ProductReceived(id, 10, DateTime.Now),
            new ProductShipped(id, 6, DateTime.Now),
            new InventoryAdjusted(id, 20, "Found more items", DateTime.Now),
        });

        var warehouseProduct = await _sut.GetByIdAsync(id);

        warehouseProduct.ShouldNotBeNull();
        warehouseProduct.QuantityOnHand.ShouldBe(24);
    }
}

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