using Microsoft.Extensions.Caching.Memory;
using ServerApp.Application.Interfaces;
using ServerApp.Domain;

namespace ServerApp.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IMemoryCache cache)
    : IProductService
{
    // Cache keys
    private const string ProductsCacheKey = "products:all";
    private const string ProductByIdKey = "products:id:"; // append id
    private const string ProductsByCategoryKey = "products:category:"; // append categoryId
    private const string SearchProductsKey = "products:search:"; // append search term

    private readonly MemoryCacheEntryOptions _defaultCacheOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

    public async Task<Product> CreateAsync(Product product)
    {
        ValidateProduct(product, true);

        // Ensure category exists
        var category = await categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category does not exist.");

        await productRepository.AddAsync(product);
        await productRepository.SaveChangesAsync();

        // Attach the category navigation so mapping includes CategoryName
        product.Category = category;

        // Invalidate caches
        cache.Remove(ProductsCacheKey);
        cache.Remove(ProductsByCategoryKey + product.CategoryId);

        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await productRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        productRepository.Delete(existing);
        await productRepository.SaveChangesAsync();

        // Invalidate caches
        cache.Remove(ProductsCacheKey);
        cache.Remove(ProductByIdKey + id);
        cache.Remove(ProductsByCategoryKey + existing.CategoryId);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        if (cache.TryGetValue(ProductsCacheKey, out IEnumerable<Product>? cached)) return cached!;
        var list = (await productRepository.GetAllAsync()).ToList();
        cache.Set(ProductsCacheKey, list, _defaultCacheOptions);
        cached = list;
        return cached;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var key = ProductByIdKey + id;
        if (cache.TryGetValue(key, out Product? cached)) return cached;
        cached = await productRepository.GetByIdAsync(id);
        if (cached != null)
            cache.Set(key, cached, _defaultCacheOptions);
        return cached;
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        var key = ProductsByCategoryKey + categoryId;
        if (cache.TryGetValue(key, out IEnumerable<Product>? cached)) return cached!;
        var list = (await productRepository.GetByCategoryIdAsync(categoryId)).ToList();
        cache.Set(key, list, _defaultCacheOptions);
        cached = list;
        return cached;
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string? name)
    {
        var key = SearchProductsKey + (name ?? string.Empty);
        if (cache.TryGetValue(key, out IEnumerable<Product>? cached)) return cached!;
        var list = await productRepository.SearchByNameAsync(name ?? string.Empty);
        var enumerable = list as Product[] ?? list.ToArray();
        cache.Set(key, enumerable, _defaultCacheOptions);
        cached = enumerable;
        return cached;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        ValidateProduct(product, false);

        var existing = await productRepository.GetByIdAsync(product.Id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        // Ensure category exists
        var category = await categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category does not exist.");

        var previousCategoryId = existing.CategoryId;
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.Category = category; // attach navigation so DTO has CategoryName

        productRepository.Update(existing);
        await productRepository.SaveChangesAsync();

        // Invalidate caches
        cache.Remove(ProductsCacheKey);
        cache.Remove(ProductByIdKey + product.Id);
        // remove cache for both old and new category lists
        cache.Remove(ProductsByCategoryKey + previousCategoryId);
        cache.Remove(ProductsByCategoryKey + product.CategoryId);

        return existing;
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? search = null, int? categoryId = null)
    {
        // Delegate to repository for paged data. Don't cache paged responses to keep logic simple.
        return await productRepository.GetPagedAsync(pageNumber, pageSize, search, categoryId);
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