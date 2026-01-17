using ServerApp.Domain;

namespace ServerApp.Application.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Product>> SearchByNameAsync(string name);
}

