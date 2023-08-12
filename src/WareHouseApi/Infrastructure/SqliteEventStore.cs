using Microsoft.Data.Sqlite;
using System.Text.Json;
using WareHouseApi.Domain;

namespace WareHouseApi.Infrastructure;

public class SqliteEventStore : IEventStore, IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteEventStore(SqliteConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }

    public async Task SaveEvents(AggregateRoot aggregate, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        var persistedEvents = await GetEvents(aggregate, cancellationToken);
        var lastEventId = persistedEvents?.LastOrDefault()?.EventId;

        using var transaction = await _connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var pendingEvent in events)
            {
                var data = JsonSerializer.Serialize(pendingEvent, pendingEvent.GetType());

                var command = _connection.CreateCommand();
                command.CommandText =
                    $@"
                INSERT INTO events(entity, entityKey, event, data, eventId, commandId{(lastEventId != null ? ", previousId" : "")}) 
                VALUES (@entity, @entityKey, @event, @data, @eventId, @commandId{(lastEventId != null ? ", @previousId" : "")});
                ";

                var eventId = Guid.NewGuid();
                command.Parameters.AddWithValue("@entity", aggregate.GetType().Name);
                command.Parameters.AddWithValue("@entityKey", aggregate.Id.ToString());
                command.Parameters.AddWithValue("@event", pendingEvent.GetType().Name);
                command.Parameters.AddWithValue("@data", data);
                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@commandId", Guid.NewGuid().ToString());
                if (lastEventId != null)
                {
                    command.Parameters.AddWithValue("@previousId", lastEventId.ToString());
                }

                lastEventId = eventId;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetEvents(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        var command = _connection.CreateCommand();
        command.CommandText =
            @"
            SELECT event, data, eventId
            FROM events
            WHERE entityKey = @entityKey AND entity = @entityName
            ORDER BY sequence
            ";

        command.Parameters.AddWithValue("@entityKey", aggregate.Id.ToString());
        command.Parameters.AddWithValue("@entityName", aggregate.GetType().Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows)
        {
            return Enumerable.Empty<Event>();
        }

        var events = new List<Event>();

        while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
        {
            var eventName = reader.GetString(0);
            var eventData = reader.GetString(1);
            var eventId = reader.GetString(2);

            var type = EventTypeResolver.GetEventType(eventName);
            if (JsonSerializer.Deserialize(eventData, type) is Event @event)
            {
                @event.EventId = Guid.Parse(eventId);
                events.Add(@event);
            }
        }

        return events;
    }

    public ValueTask DisposeAsync()
    {
        if (_connection == null)
        {
            return ValueTask.CompletedTask;
        }

        return _connection.DisposeAsync();
    }
}
