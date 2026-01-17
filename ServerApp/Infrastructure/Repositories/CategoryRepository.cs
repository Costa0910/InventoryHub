using Microsoft.EntityFrameworkCore;
using ServerApp.Application.Interfaces;
using ServerApp.Domain;
using ServerApp.Infrastructure.Persistence;

namespace ServerApp.Infrastructure.Repositories;

public class CategoryRepository(InventoryDbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    public async Task<Category?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }
}
