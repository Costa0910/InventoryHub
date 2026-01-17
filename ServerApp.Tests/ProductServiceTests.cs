using Microsoft.Extensions.Caching.Memory;
using ServerApp.Application.Services;
using ServerApp.Domain;
using ServerApp.Infrastructure.Repositories;
using ServerApp.Tests.TestHelpers;

namespace ServerApp.Tests;

public class ProductServiceTests
{
    [Test]
    public async Task CreateProduct_Fails_WhenCategoryMissing()
    {
        var tuple = SqliteInMemoryFactory.CreateDbContext();
        await using var context = tuple.context;
        await using var connection = tuple.connection;

        var productRepo = new ProductRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductService(productRepo, categoryRepo, cache);

        var product = new Product { Name = "Gadget", Price = 9.99M, Stock = 10, CategoryId = 999 };
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CreateAsync(product));
    }

    [Test]
    public async Task CreateProduct_Succeeds_WhenValid()
    {
        var tuple = SqliteInMemoryFactory.CreateDbContext();
        await using var context = tuple.context;
        await using var connection = tuple.connection;

        var productRepo = new ProductRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var cat = new Category { Name = "Gadgets" };
        await categoryRepo.AddAsync(cat);
        await categoryRepo.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductService(productRepo, categoryRepo, cache);
        var product = new Product { Name = "Gizmo", Price = 19.99M, Stock = 5, CategoryId = cat.Id };
        var created = await service.CreateAsync(product);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That((await productRepo.GetAllAsync()).Any(p => p.Name == "Gizmo"));
        }
    }

    [Test]
    public async Task CreateProduct_Fails_WhenInvalidPriceOrStock()
    {
        var tuple = SqliteInMemoryFactory.CreateDbContext();
        await using var context = tuple.context;
        await using var connection = tuple.connection;

        var productRepo = new ProductRepository(context);
        var categoryRepo = new CategoryRepository(context);
        var cat = new Category { Name = "Gadgets" };
        await categoryRepo.AddAsync(cat);
        await categoryRepo.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductService(productRepo, categoryRepo, cache);
        var negativePrice = new Product { Name = "Bad", Price = -1M, Stock = 1, CategoryId = cat.Id };
        Assert.ThrowsAsync<ArgumentException>(async () => await service.CreateAsync(negativePrice));

        var negativeStock = new Product { Name = "Bad2", Price = 1M, Stock = -5, CategoryId = cat.Id };
        Assert.ThrowsAsync<ArgumentException>(async () => await service.CreateAsync(negativeStock));
    }
}
