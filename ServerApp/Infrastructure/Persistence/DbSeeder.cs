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

        // Create 30 sample categories
        var categoryNames = new[]
        {
            "Electronics","Office","Home","Kitchen","Sports","Outdoors","Toys","Clothing","Books","Beauty",
            "Automotive","Garden","Health","Pet Supplies","Baby","Grocery","Tools","Furniture","Jewelry","Music",
            "Movies","Games","Software","Industrial","Art","Craft","Luggage","Footwear","Watches","Accessories"
        };

        var categories = categoryNames.Select(n => new Category { Name = n }).ToList();
        await context.Categories.AddRangeAsync(categories);

        // Create 50 sample products, assigning them to categories round-robin
        var adjectives = new[] { "Portable", "Advanced", "Classic", "Smart", "Compact", "Deluxe", "Eco", "Premium", "Mini", "Pro", "Ultra", "Lightweight", "Durable", "Wireless", "Rechargeable" };
        var nouns = new[] { "Speaker", "Headphones", "Camera", "Blender", "Backpack", "Sneakers", "Watch", "Lamp", "Thermostat", "Drill", "Mixer", "Tablet", "Chair", "Book", "Game", "Router", "Monitor", "Keyboard", "Mouse", "Printer" };

        var products = new List<Product>(capacity: 50);
        for (var i = 1; i <= 50; i++)
        {
            var adj = adjectives[(i - 1) % adjectives.Length];
            var noun = nouns[(i - 1) % nouns.Length];
            var name = $"{adj} {noun} {i}";
            var description = $"Sample product {i}: {adj} {noun} for everyday use.";
            var price = Math.Round(5 + (decimal)(i * 2.35), 2);
            var stock = 10 + (i * 3) % 100;
            var category = categories[(i - 1) % categories.Count];

            products.Add(new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                Category = category
            });
        }

        await context.Products.AddRangeAsync(products);

        await context.SaveChangesAsync();
    }
}
