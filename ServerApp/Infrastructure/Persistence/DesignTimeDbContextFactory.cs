using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServerApp.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<InventoryDbContext>();
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");
        builder.UseSqlite($"Data Source={dbPath}");
        return new InventoryDbContext(builder.Options);
    }
}

