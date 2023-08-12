namespace WareHouseApi.Domain;

public abstract class AggregateRoot
{
    protected AggregateRoot(Guid id)
        => Id = id;

    public Guid Id { get; }

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
