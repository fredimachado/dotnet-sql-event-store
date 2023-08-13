using Microsoft.Data.Sqlite;
using WareHouseApi.Domain.Entities;
using WareHouseApi.Domain.Events;
using WareHouseApi.Infrastructure;

namespace Warehouse.Tests.Infrastructure;

public class SqliteEventStoreTests : IClassFixture<SqliteFixture>, IAsyncDisposable
{
    private readonly SqliteEventStore _eventStore;
    private readonly SqliteFixture _fixture;

    public SqliteEventStoreTests(SqliteFixture fixture)
    {
        _fixture = fixture;
        _eventStore = new SqliteEventStore(new SqliteConnection(SqliteFixture.ConnectionString));
    }

    [Fact]
    public async Task Test_Saving_Event()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);
        warehouseProduct.ReceiveProduct(10);

        await _eventStore.SaveEventsAsync(warehouseProduct, warehouseProduct.GetPendingEvents());

        var events = await _eventStore.GetEventsAsync(warehouseProduct);

        var @event = events.ShouldHaveSingleItem();
        var productReceived = @event.ShouldBeOfType<ProductReceived>();
        productReceived.Quantity.ShouldBe(10);
    }

    public async ValueTask DisposeAsync()
    {
        await _eventStore.DisposeAsync();
        await _fixture.DisposeAsync();
    }
}
