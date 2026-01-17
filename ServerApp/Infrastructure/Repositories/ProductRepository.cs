using Microsoft.EntityFrameworkCore;
using ServerApp.Application.Interfaces;
using ServerApp.Domain;
using ServerApp.Infrastructure.Persistence;

namespace ServerApp.Infrastructure.Repositories;

public class ProductRepository(InventoryDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet.Where(p => p.CategoryId == categoryId).Include(p => p.Category).ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return await GetAllAsync();

        return await _dbSet.Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
                           .Include(p => p.Category)
                           .ToListAsync();
    }
}

