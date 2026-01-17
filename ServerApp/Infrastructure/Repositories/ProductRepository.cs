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

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? search = null, int? categoryId = null)
    {
        var query = _dbSet.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // search in Name and Description
            var like = $"%{search}%";
            query = query.Where(p => EF.Functions.Like(p.Name, like) || EF.Functions.Like(p.Description ?? string.Empty, like));
        }

        if (categoryId.HasValue && categoryId.Value != 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }
}
