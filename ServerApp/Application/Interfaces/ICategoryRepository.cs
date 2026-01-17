using ServerApp.Domain;

namespace ServerApp.Application.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}

