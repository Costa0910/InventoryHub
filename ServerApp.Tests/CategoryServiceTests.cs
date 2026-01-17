using ServerApp.Application.Services;
using ServerApp.Domain;
using ServerApp.Infrastructure.Repositories;
using ServerApp.Tests.TestHelpers;

namespace ServerApp.Tests;

public class CategoryServiceTests
{
    [Test]
    public async Task CreateCategory_Succeeds_WhenValid()
    {
        var tuple = SqliteInMemoryFactory.CreateDbContext();
        await using var context = tuple.context;
        await using var connection = tuple.connection;

        var repo = new CategoryRepository(context);
        var service = new CategoryService(repo);

        var category = new ServerApp.Domain.Category { Name = "Toys" };
        var created = await service.CreateAsync(category);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That((await repo.GetAllAsync()).Any(c => c.Name == "Toys"));
        }
    }

    [Test]
    public async Task CreateCategory_Fails_WhenDuplicateName()
    {
        var tuple = SqliteInMemoryFactory.CreateDbContext();
        await using var context = tuple.context;
        await using var connection = tuple.connection;

        var repo = new CategoryRepository(context);
        var service = new CategoryService(repo);

        var category = new Category { Name = "Toys" };
        await service.CreateAsync(category);

        var duplicate = new Category { Name = "Toys" };
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CreateAsync(duplicate));
    }
}
