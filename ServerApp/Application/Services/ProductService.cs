using ServerApp.Application.Interfaces;
using ServerApp.Domain;

namespace ServerApp.Application.Services;

public class ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    : IProductService
{
    public async Task<Product> CreateAsync(Product product)
    {
        ValidateProduct(product, isNew: true);

        // Ensure category exists
        var category = await categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category does not exist.");

        await productRepository.AddAsync(product);
        await productRepository.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await productRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        productRepository.Delete(existing);
        await productRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await productRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await productRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await productRepository.GetByCategoryIdAsync(categoryId);
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        return await productRepository.SearchByNameAsync(name);
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        ValidateProduct(product, isNew: false);

        var existing = await productRepository.GetByIdAsync(product.Id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        // Ensure category exists
        var category = await categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category does not exist.");

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;

        productRepository.Update(existing);
        await productRepository.SaveChangesAsync();

        return existing;
    }

    private void ValidateProduct(Product product, bool isNew)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Product name is required");
        if (product.Price < 0) throw new ArgumentException("Price must be >= 0");
        if (product.Stock < 0) throw new ArgumentException("Stock must be >= 0");
        if (!isNew && product.Id <= 0) throw new ArgumentException("Invalid product id");
    }
}

