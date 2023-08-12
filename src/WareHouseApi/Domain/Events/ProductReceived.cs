namespace WareHouseApi.Domain.Events;

public record ProductReceived(Guid Id, int Quantity, DateTime DateTime) : Event(Id);
