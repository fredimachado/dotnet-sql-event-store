namespace WareHouseApi.Domain.Events;

public record ProductShipped(Guid Id, int Quantity, DateTime DateTime) : Event(Id);
