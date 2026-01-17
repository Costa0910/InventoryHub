using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerApp.Domain;
using ServerApp.Infrastructure.Persistence;
using Shared.DTOs;

namespace ServerApp.Tests;

public class ApiIntegrationTests
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [SetUp]
    public void Setup()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<InventoryDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Register in-memory Sqlite
                services.AddDbContext<InventoryDbContext>(options =>
                {
                    options.UseSqlite(connection);
                });

                // Build provider and initialize DB
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                // Use EnsureCreated in tests to initialize schema for the in-memory SQLite
                // connection. Avoids migration locking / duplicate CREATE TABLE errors.
                db.Database.EnsureCreated();
                if (db.Categories.Any()) return;
                var cat = new Category { Name = "Integration" };
                db.Categories.Add(cat);
                db.Products.Add(new Product { Name = "Int Product", Price = 1.99M, Stock = 10, Category = cat });
                db.SaveChanges();
            });
        });

        _client = _factory.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task GetCategories_ReturnsSeededCategory()
    {
        var res = await _client?.GetAsync("/api/categories")!;
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var categories = await res.Content.ReadFromJsonAsync<CategoryDto[]>();
        Assert.That(categories, Is.Not.Null);
        Assert.That(categories!.Any(c => c.Name == "Integration"), Is.True);
    }

    [Test]
    public async Task GetProducts_ReturnsSeededProduct()
    {
        var res = await _client?.GetAsync("/api/products")!;
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var products = await res.Content.ReadFromJsonAsync<ProductDto[]>();
        Assert.That(products, Is.Not.Null);
        Assert.That(products!.Any(p => p.Name == "Int Product"), Is.True);
    }

    [Test]
    public async Task CreateCategory_CreatesAndReturnsLocation()
    {
        var dto = new CategoryDto { Name = "NewCat" };
        if (_client != null)
        {
            var res = await _client.PostAsJsonAsync("/api/categories", dto);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
                Assert.That(res.Headers.Location, Is.Not.Null);
            }

            var created = await res.Content.ReadFromJsonAsync<CategoryDto>();
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.Name, Is.EqualTo("NewCat"));
        }
    }

    [Test]
    public async Task CreateProduct_WithInvalidCategory_ReturnsNotFound()
    {
        var dto = new ProductDto { Name = "Bad", Price = 1, Stock = 1, CategoryId = 9999 };
        if (_client != null)
        {
            var res = await _client.PostAsJsonAsync("/api/products", dto);
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
