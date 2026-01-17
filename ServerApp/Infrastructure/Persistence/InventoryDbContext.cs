using Microsoft.EntityFrameworkCore;
using ServerApp.Domain;

namespace ServerApp.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired();
            b.HasIndex(c => c.Name).HasDatabaseName("IX_Categories_Name");
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired();
            b.Property(p => p.Price).HasPrecision(18, 2);
            b.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);

            // Indexes to speed up common queries
            b.HasIndex(p => p.Name).HasDatabaseName("IX_Products_Name");
            b.HasIndex(p => p.CategoryId).HasDatabaseName("IX_Products_CategoryId");
        });
    }
}
