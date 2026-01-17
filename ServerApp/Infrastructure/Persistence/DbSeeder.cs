using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerApp.Domain;

namespace ServerApp.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger(typeof(DbSeeder).FullName ?? "DbSeeder");

        // Try to apply any pending migrations;
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (InvalidOperationException ex)
        {
            logger?.LogWarning(ex, "Migrations could not be applied; falling back to EnsureCreated.");
            await context.Database.EnsureCreatedAsync();
        }

        if (await context.Categories.AnyAsync())
            return; // already seeded

        var cat1 = new Category { Name = "Electronics" };
        var cat2 = new Category { Name = "Office" };
        var cat3 = new Category { Name = "Home" };

        await context.Categories.AddRangeAsync(cat1, cat2, cat3);

        var products = new[]
        {
            new Product { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 29.99M, Stock = 50, Category = cat1},
            new Product { Name = "Mechanical Keyboard", Description = "Tactile mechanical keyboard", Price = 89.99M, Stock = 30, Category = cat1},
            new Product { Name = "Stapler", Description = "Standard stapler", Price = 6.49M, Stock = 150, Category = cat2},
            new Product { Name = "Desk Lamp", Description = "LED desk lamp", Price = 24.99M, Stock = 40, Category = cat3}
        };

        await context.Products.AddRangeAsync(products);

        await context.SaveChangesAsync();
    }
}
