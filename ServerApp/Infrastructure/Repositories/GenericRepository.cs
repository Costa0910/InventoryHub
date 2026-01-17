using Microsoft.EntityFrameworkCore;
using ServerApp.Application.Interfaces;
using ServerApp.Infrastructure.Persistence;

namespace ServerApp.Infrastructure.Repositories;

public class GenericRepository<T>(InventoryDbContext context) : IGenericRepository<T>
    where T : class
{
    protected readonly InventoryDbContext Context = context;
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
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        // FindAsync returns tracked entity; use FindAsync then detach to avoid tracking costs if desired
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }
}
