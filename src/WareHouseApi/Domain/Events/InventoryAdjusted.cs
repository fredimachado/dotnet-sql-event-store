namespace WareHouseApi.Domain.Events;

public record InventoryAdjusted(Guid Id, int Quantity, string Reason, DateTime DateTime) : Event(Id);
