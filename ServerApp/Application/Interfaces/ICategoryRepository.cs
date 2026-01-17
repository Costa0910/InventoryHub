using ServerApp.Domain;

namespace ServerApp.Application.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<(IEnumerable<Category> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? search = null);
}
