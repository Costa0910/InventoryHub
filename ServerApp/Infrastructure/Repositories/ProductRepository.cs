using Microsoft.EntityFrameworkCore;
using ServerApp.Application.Interfaces;
using ServerApp.Domain;
using ServerApp.Infrastructure.Persistence;

namespace ServerApp.Infrastructure.Repositories;

public class ProductRepository(InventoryDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    // Override GetAllAsync to include Category navigation
    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().Include(p => p.Category).ToListAsync();
    }

    // Override GetByIdAsync to include Category navigation
    public override async Task<Product?> GetByIdAsync(object? id)
    {
        if (id == null) return null;
        var key = Convert.ToInt32(id);
        return await _dbSet.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == key);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet.AsNoTracking().Where(p => p.CategoryId == categoryId).Include(p => p.Category).ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return await GetAllAsync();

        return await _dbSet.AsNoTracking()
                           .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
                           .Include(p => p.Category)
                           .ToListAsync();
    }
}
