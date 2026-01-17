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

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<(IEnumerable<Category> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? search = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = $"%{search}%";
            query = query.Where(c => EF.Functions.Like(c.Name, like));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }
}
