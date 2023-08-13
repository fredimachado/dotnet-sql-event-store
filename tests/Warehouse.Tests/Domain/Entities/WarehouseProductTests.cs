using WareHouseApi.Domain;
using WareHouseApi.Domain.Entities;
using WareHouseApi.Domain.Events;

namespace Warehouse.Tests.Domain.Entities;

public class WarehouseProductTests
{
    [Fact]
    public void ReceiveProduct()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);

        warehouseProduct.ReceiveProduct(10);

        var @event = warehouseProduct.GetPendingEvents().ShouldHaveSingleItem();
        @event.ShouldBeOfType<ProductReceived>();

        warehouseProduct.Id.ShouldBe(id);
        warehouseProduct.QuantityOnHand.ShouldBe(10);
    }

    [Fact]
    public void ShipProduct()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);

        warehouseProduct.ReceiveProduct(10);
        warehouseProduct.ShipProduct(6);

        warehouseProduct.GetPendingEvents().Count.ShouldBe(2);
        warehouseProduct.QuantityOnHand.ShouldBe(4);
    }

    [Fact]
    public void AdjustInventory()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);

        warehouseProduct.ReceiveProduct(10);
        warehouseProduct.ShipProduct(6);
        warehouseProduct.AdjustInventory(20, "Found more 20 items");

        warehouseProduct.GetPendingEvents().Count.ShouldBe(3);
        warehouseProduct.QuantityOnHand.ShouldBe(24);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReceiveInvalidQuantity(int quantity)
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);

        var result = Record.Exception(() => warehouseProduct.ReceiveProduct(quantity));

        result.ShouldBeOfType<InvalidDomainException>();
    }

    [Fact]
    public void ShipInvalidQuantity()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);

        warehouseProduct.ReceiveProduct(10);

        var result = Record.Exception(() => warehouseProduct.ShipProduct(11));

        result.ShouldBeOfType<InvalidDomainException>();
    }

    [Fact]
    public void AdjustInvalidQuantity()
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new WarehouseProduct(id);

        warehouseProduct.ReceiveProduct(10);

        var result = Record.Exception(() => warehouseProduct.AdjustInventory(-11, "fail"));

        result.ShouldBeOfType<InvalidDomainException>();
    }
}
