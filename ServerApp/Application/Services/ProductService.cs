using Microsoft.Extensions.Caching.Memory;
using ServerApp.Application.Interfaces;
using ServerApp.Domain;

namespace ServerApp.Application.Services;

public class ProductService : IProductService
{
    // Cache keys
    private const string ProductsCacheKey = "products:all";
    private const string ProductByIdKey = "products:id:"; // append id
    private const string ProductsByCategoryKey = "products:category:"; // append categoryId
    private const string SearchProductsKey = "products:search:"; // append search term
    private readonly IMemoryCache _cache;
    private readonly ICategoryRepository _categoryRepository;

    private readonly MemoryCacheEntryOptions _defaultCacheOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository,
        IMemoryCache cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        ValidateProduct(product, true);

        // Ensure category exists
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category does not exist.");

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        // Attach the category navigation so mapping includes CategoryName
        product.Category = category;

        // Invalidate caches
        _cache.Remove(ProductsCacheKey);
        _cache.Remove(ProductsByCategoryKey + product.CategoryId);

        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        _productRepository.Delete(existing);
        await _productRepository.SaveChangesAsync();

        // Invalidate caches
        _cache.Remove(ProductsCacheKey);
        _cache.Remove(ProductByIdKey + id);
        _cache.Remove(ProductsByCategoryKey + existing.CategoryId);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        if (_cache.TryGetValue(ProductsCacheKey, out IEnumerable<Product>? cached)) return cached!;
        var list = (await _productRepository.GetAllAsync()).ToList();
        _cache.Set(ProductsCacheKey, list, _defaultCacheOptions);
        cached = list;
        return cached;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var key = ProductByIdKey + id;
        if (_cache.TryGetValue(key, out Product? cached)) return cached;
        cached = await _productRepository.GetByIdAsync(id);
        if (cached != null)
            _cache.Set(key, cached, _defaultCacheOptions);
        return cached;
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        var key = ProductsByCategoryKey + categoryId;
        if (_cache.TryGetValue(key, out IEnumerable<Product>? cached)) return cached!;
        var list = (await _productRepository.GetByCategoryIdAsync(categoryId)).ToList();
        _cache.Set(key, list, _defaultCacheOptions);
        cached = list;
        return cached;
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string? name)
    {
        var key = SearchProductsKey + (name ?? string.Empty);
        if (_cache.TryGetValue(key, out IEnumerable<Product>? cached)) return cached!;
        var list = await _productRepository.SearchByNameAsync(name ?? string.Empty);
        var enumerable = list as Product[] ?? list.ToArray();
        _cache.Set(key, enumerable, _defaultCacheOptions);
        cached = enumerable;
        return cached;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        ValidateProduct(product, false);

        var existing = await _productRepository.GetByIdAsync(product.Id);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        // Ensure category exists
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category does not exist.");

        var previousCategoryId = existing.CategoryId;
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.Category = category; // attach navigation so DTO has CategoryName

        _productRepository.Update(existing);
        await _productRepository.SaveChangesAsync();

        // Invalidate caches
        _cache.Remove(ProductsCacheKey);
        _cache.Remove(ProductByIdKey + product.Id);
        // remove cache for both old and new category lists
        _cache.Remove(ProductsByCategoryKey + previousCategoryId);
        _cache.Remove(ProductsByCategoryKey + product.CategoryId);

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