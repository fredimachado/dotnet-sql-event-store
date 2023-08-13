using Microsoft.Data.Sqlite;

namespace Warehouse.Tests.EventStore;

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
