using Microsoft.Data.Sqlite;
using WareHouseApi.Domain;
using WareHouseApi.Infrastructure;

namespace Warehouse.Tests.Infrastructure;

public class SqliteFixture : IAsyncDisposable
{
    public const string ConnectionString = "Data Source=EventStore;Mode=Memory;Cache=Shared";

    private readonly SqliteConnection _sqliteConnection;
    public SqliteFixture()
    {
        EventTypeResolver.RegisterEvents(typeof(Event).Assembly);

        _sqliteConnection = new SqliteConnection(ConnectionString);
        _sqliteConnection.Open();

        RunInitializationScript();

        AddEntityEvents();
    }

    private void AddEntityEvents()
    {
        var entityName = "WarehouseProduct";
        var commands = $@"
            INSERT INTO entity_events (entity, event) VALUES ('{entityName}', 'ProductReceived');
            INSERT INTO entity_events (entity, event) VALUES ('{entityName}', 'ProductShipped');
            INSERT INTO entity_events (entity, event) VALUES ('{entityName}', 'InventoryAdjusted');
            ";
        var command = new SqliteCommand(commands, _sqliteConnection);
        command.ExecuteNonQuery();
    }

    private void RunInitializationScript()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "sqlite-event-store.ddl");
        var commands = File.ReadAllText(path);

        var command = new SqliteCommand(commands, _sqliteConnection);
        command.ExecuteNonQuery();
    }

    public ValueTask DisposeAsync()
    {
        return _sqliteConnection.DisposeAsync();
    }
}
