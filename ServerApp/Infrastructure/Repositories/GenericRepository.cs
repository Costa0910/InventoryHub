using Microsoft.EntityFrameworkCore;
using ServerApp.Application.Interfaces;
using ServerApp.Infrastructure.Persistence;

namespace ServerApp.Infrastructure.Repositories;

public class GenericRepository<T>(InventoryDbContext context) : IGenericRepository<T>
    where T : class
{
    protected readonly InventoryDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

