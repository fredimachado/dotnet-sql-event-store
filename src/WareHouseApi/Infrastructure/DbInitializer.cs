using Microsoft.Data.Sqlite;

namespace WareHouseApi.Infrastructure;

public static class DbInitializer
{
    public static void Init(string connectionString)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        if (File.Exists(Path.Combine(currentDirectory, "warehouse.db")))
        {
            return;
        }

        var path = Path.Combine(currentDirectory, "..", "..", "tests", "sqlite-event-store.ddl");
        var commands = File.ReadAllText(path);

        Execute(commands, connectionString);
    }

    public static void AddEntityEvent(string entityName, string eventName, string connectionString)
    {
        Execute($"INSERT INTO entity_events (entity, event) VALUES ('{entityName}', '{eventName}')", connectionString);
    }

    private static void Execute(string commandText, string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = new SqliteCommand(commandText, connection);
        command.ExecuteNonQuery();
    }
}
