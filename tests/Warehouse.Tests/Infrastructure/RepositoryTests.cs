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
    public async Task GetById_Should_Return_Aggregate_With_All_Events_Applied()
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

    [Fact]
    public async Task Successful_Save_Should_Clear_Pending_Events()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);
        warehouseProduct.ReceiveProduct(10);
        warehouseProduct.ShipProduct(6);

        warehouseProduct.GetPendingEvents().Count.ShouldBe(2);

        await _sut.SaveAsync(warehouseProduct);

        warehouseProduct.GetPendingEvents().Count.ShouldBe(0);
    }
}
