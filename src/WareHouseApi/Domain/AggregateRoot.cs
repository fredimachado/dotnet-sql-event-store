namespace WareHouseApi.Domain;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }

    private readonly List<Event> _pendingEvents = new();

    protected void RaiseEvent(Event @event)
    {
        ApplyEvent(@event);

        _pendingEvents.Add(@event);
    }

    public IReadOnlyList<Event> GetPendingEvents()
    {
        return _pendingEvents.AsReadOnly();
    }

    internal void ClearPendingEvents()
    {
        _pendingEvents.Clear();
    }

    internal void ApplyEvent(Event @event)
    {
        ((dynamic)this).Apply((dynamic)@event);
    }
}
