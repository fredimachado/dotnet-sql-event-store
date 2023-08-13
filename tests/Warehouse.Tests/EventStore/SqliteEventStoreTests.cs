using Microsoft.Data.Sqlite;
using WareHouseApi.Infrastructure;

namespace Warehouse.Tests.EventStore;

public class SqliteEventStoreTests : IClassFixture<SqliteFixture>, IAsyncDisposable
{
    private readonly SqliteFixture _fixture;
    private readonly SqliteEventStore _sut;

    public SqliteEventStoreTests(SqliteFixture fixture)
    {
        _fixture = fixture;
        _sut = new SqliteEventStore(new SqliteConnection(SqliteFixture.ConnectionString));
    }

    // This is one big test because these tests must run in sequence
    // and I don't know how to do this with separate tests.
    [Fact]
    public async Task Test_Sqlite_Event_Store_Database()
    {
        // Cannot Insert Null Fields Into Entity Events Table
        // ----------------------------------------------------------------------------------------
        var result = await AddEntityEvent(entityName: null, eventName: "ThingCreated");
        var exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEntityEvent(entityName: "Thing", eventName: null);
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Can Insert Entity Events
        // ----------------------------------------------------------------------------------------
        result = await AddEntityEvent(entityName: "Thing", eventName: "ThingCreated");
        result.ShouldBeNull();

        result = await AddEntityEvent(entityName: "Thing", eventName: "ThingDeleted");
        result.ShouldBeNull();

        result = await AddEntityEvent(entityName: "TableTenis", eventName: "Ping");
        result.ShouldBeNull();

        result = await AddEntityEvent(entityName: "TableTenis", eventName: "Pong");
        result.ShouldBeNull();
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Insert Duplicate Entity Events Should Not Throw
        // ----------------------------------------------------------------------------------------
        result = await AddEntityEvent(entityName: "Thing", eventName: "ThingCreated");
        result.ShouldBeNull();

        result = await AddEntityEvent(entityName: "Thing", eventName: "ThingCreated");
        result.ShouldBeNull();

        using (var secondConnection = new SqliteConnection(SqliteFixture.ConnectionString))
        {
            secondConnection.Open();
            var queryCommand = secondConnection.CreateCommand();
            queryCommand.CommandText = "SELECT COUNT(*) FROM entity_events WHERE entity = @entityName AND event = @eventName";
            queryCommand.Parameters.AddWithValue("@entityName", "Thing");
            queryCommand.Parameters.AddWithValue("@eventName", "ThingCreated");
            var count = queryCommand.ExecuteScalar();

            count.ShouldBe(1);
        }
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Cannot Insert Null Fields Into Events Table
        // ----------------------------------------------------------------------------------------
        result = await AddEventWithPreviousId(entityName: null, "12345", "ThingCreated", "data", "71095e38-d21a-454e-a777-926001bae94a", "5fa265ab-e4b7-413f-ad4b-052d41ae2999", "previousId1");
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEventWithPreviousId("Thing", entityKey: null, "ThingCreated", "data", "71095e38-d21a-454e-a777-926001bae94a", "5fa265ab-e4b7-413f-ad4b-052d41ae2999", "previousId1");
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEventWithPreviousId("Thing", "12345", eventName: null, "data", "71095e38-d21a-454e-a777-926001bae94a", "5fa265ab-e4b7-413f-ad4b-052d41ae2999", "previousId1");
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEventWithPreviousId("Thing", "12345", "ThingCreated", data: null, "71095e38-d21a-454e-a777-926001bae94a", "5fa265ab-e4b7-413f-ad4b-052d41ae2999", "previousId1");
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEventWithPreviousId("Thing", "12345", "ThingCreated", "data", eventId: null, "5fa265ab-e4b7-413f-ad4b-052d41ae2999", "previousId1");
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEventWithPreviousId("Thing", "12345", "ThingCreated", "data", "71095e38-d21a-454e-a777-926001bae94a", commandId: null, "previousId1");
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");

        result = await AddEventWithPreviousId("Thing", "12345", "ThingCreated", "data", "71095e38-d21a-454e-a777-926001bae94a", "5fa265ab-e4b7-413f-ad4b-052d41ae2999", previousId: null);
        exception = result.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldBe("Value must be set.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Cannot Insert Non Guid Values Into Id Columns In Events Table
        // ----------------------------------------------------------------------------------------
        result = await AddEvent("Thing", "12345", "ThingCreated", "data", "not-a-guid", "5fa265ab-e4b7-413f-ad4b-052d41ae2999");
        var sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'CHECK constraint failed: eventId LIKE '________-____-____-____-____________''.");

        result = await AddEvent("Thing", "12345", "ThingCreated", "data", "5fa265ab-e4b7-413f-ad4b-052d41ae2999", "not-a-guid");
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'CHECK constraint failed: commandId LIKE '________-____-____-____-____________''.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Cannot Insert Event From Wrong Entity
        // ----------------------------------------------------------------------------------------
        result = await AddEvent("Thing", "12345", "Ping", "data", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'FOREIGN KEY constraint failed'.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Insert Valid Events Should Not Throw
        // ----------------------------------------------------------------------------------------
        var firstEventId = Guid.NewGuid().ToString();
        var commandId = Guid.NewGuid().ToString();
        result = await AddEvent("Thing", "1", "ThingCreated", "{}", firstEventId, commandId);
        result.ShouldBeNull();

        var thingEventId = Guid.NewGuid().ToString();
        var thingCommandId = Guid.NewGuid().ToString();
        result = await AddEventWithPreviousId("Thing", "1", "ThingDeleted", "{}", thingEventId, thingCommandId, firstEventId);
        result.ShouldBeNull();

        var pingEventHomeId = Guid.NewGuid().ToString();
        result = await AddEvent("TableTenis", "home", "Ping", "{}", pingEventHomeId, Guid.NewGuid().ToString());
        result.ShouldBeNull();

        result = await AddEvent("TableTenis", "work", "Ping", "{}", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        result.ShouldBeNull();
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Cannot Insert Multiple Null Previous Id For An Entity
        // ----------------------------------------------------------------------------------------
        result = await AddEvent("TableTenis", "home", "Ping", "{}", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'previousId can only be null for first entity event'.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Previous Id Should Be Present After First Event
        // ----------------------------------------------------------------------------------------
        result = await AddEvent("TableTenis", "work", "Pong", "{}", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'previousId can only be null for first entity event'.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Cannot Insert Duplicates
        // ----------------------------------------------------------------------------------------
        result = await AddEventWithPreviousId("Thing", "1", "ThingCreated", "{}", thingEventId, thingCommandId, firstEventId);
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'UNIQUE constraint failed: events.previousId'.");

        result = await AddEventWithPreviousId("TableTenis", "home", "Pong", "{}", pingEventHomeId, Guid.NewGuid().ToString(), pingEventHomeId);
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'UNIQUE constraint failed: events.eventId'.");

        result = await AddEventWithPreviousId("Thing", "1", "ThingDeleted", "{}", Guid.NewGuid().ToString(), commandId, thingEventId);
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'UNIQUE constraint failed: events.commandId'.");

        result = await AddEventWithPreviousId("Thing", "1", "ThingDeleted", "{}", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), firstEventId);
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'UNIQUE constraint failed: events.previousId'.");
        // ----------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------

        // Cannot Delete Or Update
        // ----------------------------------------------------------------------------------------
        result = await DeleteEntityEvents("TableTenis");
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'Cannot delete entity_events'.");

        result = await DeleteEvents("Thing");
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'Cannot delete events'.");

        result = await UpdateEntityEvents("TableTenis");
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'Cannot update entity_events'.");

        result = await UpdateEvents("Thing");
        sqliteException = result.ShouldBeOfType<SqliteException>();
        sqliteException.Message.ShouldBe("SQLite Error 19: 'Cannot update events'.");
    }

    private Task<Exception> UpdateEvents(string entityName)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("UPDATE events SET entityKey = 'fail' WHERE entity = @entityName", connection);
        command.Parameters.AddWithValue("@entityName", entityName);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    private Task<Exception> DeleteEvents(string entityName)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("DELETE FROM events WHERE entity = @entityName", connection);
        command.Parameters.AddWithValue("@entityName", entityName);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    private Task<Exception> UpdateEntityEvents(string entityName)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("UPDATE entity_events SET entity = 'fail' WHERE entity = @entityName", connection);
        command.Parameters.AddWithValue("@entityName", entityName);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    private Task<Exception> DeleteEntityEvents(string entityName)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("DELETE FROM entity_events WHERE entity = @entityName", connection);
        command.Parameters.AddWithValue("@entityName", entityName);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    private Task<Exception> AddEntityEvent(string entityName, string eventName)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("INSERT INTO entity_events (entity, event) VALUES (@entityName, @eventName)", connection);
        command.Parameters.AddWithValue("@entityName", entityName);
        command.Parameters.AddWithValue("@eventName", eventName);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    private Task<Exception> AddEvent(string entityName, string entityKey, string eventName, string data, string eventId, string commandId)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("INSERT INTO events (entity, entityKey, event, data, eventId, commandId) VALUES (@entityName, @entityKey, @eventName, @data, @eventId, @commandId)", connection);
        command.Parameters.AddWithValue("@entityName", entityName);
        command.Parameters.AddWithValue("@entityKey", entityKey);
        command.Parameters.AddWithValue("@eventName", eventName);
        command.Parameters.AddWithValue("@data", data);
        command.Parameters.AddWithValue("@eventId", eventId);
        command.Parameters.AddWithValue("@commandId", commandId);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    private Task<Exception> AddEventWithPreviousId(string entityName, string entityKey, string eventName, string data, string eventId, string commandId, string previousId)
    {
        using var connection = new SqliteConnection(SqliteFixture.ConnectionString);
        connection.Open();

        var command = new SqliteCommand("INSERT INTO events (entity, entityKey, event, data, eventId, commandId, previousId) VALUES (@entityName, @entityKey, @eventName, @data, @eventId, @commandId, @previousId)", connection);
        command.Parameters.AddWithValue("@entityName", entityName);
        command.Parameters.AddWithValue("@entityKey", entityKey);
        command.Parameters.AddWithValue("@eventName", eventName);
        command.Parameters.AddWithValue("@data", data);
        command.Parameters.AddWithValue("@eventId", eventId);
        command.Parameters.AddWithValue("@commandId", commandId);
        command.Parameters.AddWithValue("@previousId", previousId);

        return Record.ExceptionAsync(command.ExecuteNonQueryAsync);
    }

    public ValueTask DisposeAsync()
    {
        return _sut.DisposeAsync();
    }
}

public class SqliteFixture : IDisposable
{
    public const string ConnectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

    private readonly SqliteConnection _sqliteConnection;
    public SqliteFixture()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "sqlite-event-store.ddl");
        var commands = File.ReadAllText(path);

        _sqliteConnection = new SqliteConnection(ConnectionString);
        _sqliteConnection.Open();

        var command = new SqliteCommand(commands, _sqliteConnection);
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _sqliteConnection.Dispose();
    }
}