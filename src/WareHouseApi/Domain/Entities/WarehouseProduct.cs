using WareHouseApi.Domain.Events;

namespace WareHouseApi.Domain.Entities;

public sealed class WarehouseProduct : AggregateRoot
{
    public WarehouseProduct(Guid id) : base(id)
    {
    }

    public int QuantityOnHand { get; private set; }

    public void ReceiveProduct(int quantity)
    {
        RaiseEvent(new ProductReceived(Id, quantity, DateTime.UtcNow));
    }

    public void ShipProduct(int quantity)
    {
        if (quantity > QuantityOnHand)
        {
            throw new InvalidDomainException("There's not enough products to ship.");
        }

        RaiseEvent(new ProductShipped(Id, quantity, DateTime.UtcNow));
    }

    public void AdjustInventory(int quantity, string reason)
    {
        if (QuantityOnHand + quantity < 0)
        {
            throw new InvalidDomainException("Quantity on hand cannot be negative.");
        }

        RaiseEvent(new InventoryAdjusted(Id, quantity, reason, DateTime.UtcNow));
    }

    public void Apply(ProductReceived @event)
    {
        QuantityOnHand += @event.Quantity;
    }

    public void Apply(ProductShipped @event)
    {
        QuantityOnHand -= @event.Quantity;
    }

    public void Apply(InventoryAdjusted @event)
    {
        QuantityOnHand += @event.Quantity;
    }
}
