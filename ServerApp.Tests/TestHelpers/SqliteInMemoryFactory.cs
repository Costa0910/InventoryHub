using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ServerApp.Infrastructure.Persistence;

namespace ServerApp.Tests.TestHelpers;

public static class SqliteInMemoryFactory
{
    public static (InventoryDbContext context, SqliteConnection connection) CreateDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new InventoryDbContext(options);
        context.Database.EnsureCreated();
        return (context, connection);
    }
}

