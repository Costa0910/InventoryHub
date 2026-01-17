using Microsoft.Extensions.Caching.Memory;
using ServerApp.Application.Interfaces;
using ServerApp.Domain;

namespace ServerApp.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository, IMemoryCache cache) : ICategoryService
{
    private const string CategoriesCacheKey = "categories:all";
    private const string CategoryByIdKey = "categories:id:";
    private const string CategoryByNameKey = "categories:name:";

    private readonly MemoryCacheEntryOptions _defaultCacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

    public async Task<Category> CreateAsync(Category category)
    {
        ValidateCategory(category, isNew: true);

        // Prevent duplicate names
        var existing = await categoryRepository.GetByNameAsync(category.Name);
        if (existing != null)
            throw new InvalidOperationException("Category with the same name already exists.");

        await categoryRepository.AddAsync(category);
        await categoryRepository.SaveChangesAsync();

        cache.Remove(CategoriesCacheKey);
        cache.Remove(CategoryByNameKey + category.Name);

        return category;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await categoryRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Category not found");

        categoryRepository.Delete(existing);
        await categoryRepository.SaveChangesAsync();

        cache.Remove(CategoriesCacheKey);
        cache.Remove(CategoryByIdKey + id);
        cache.Remove(CategoryByNameKey + existing.Name);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        if (cache.TryGetValue(CategoriesCacheKey, out IEnumerable<Category>? cachedValue)) return cachedValue!;
        cachedValue = await categoryRepository.GetAllAsync();
        var categories = cachedValue as Category[] ?? cachedValue.ToArray();
        cache.Set(CategoriesCacheKey,categories, _defaultCacheOptions);
        return categories!;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var key = CategoryByIdKey + id;
        if (cache.TryGetValue(key, out Category? cached)) return cached;
        cached = await categoryRepository.GetByIdAsync(id);
        if (cached != null)
            cache.Set(key, cached, _defaultCacheOptions);
        return cached;
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var key = CategoryByNameKey + name;
        if (cache.TryGetValue(key, out Category? cached)) return cached;
        cached = await categoryRepository.GetByNameAsync(name);
        if (cached != null)
            cache.Set(key, cached, _defaultCacheOptions);
        return cached;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        ValidateCategory(category, isNew: false);

        var existing = await categoryRepository.GetByIdAsync(category.Id);
        if (existing == null)
            throw new KeyNotFoundException("Category not found");

        // Prevent renaming to a name that already exists
        var byName = await categoryRepository.GetByNameAsync(category.Name);
        if (byName != null && byName.Id != category.Id)
            throw new InvalidOperationException("Category with the same name already exists.");

        existing.Name = category.Name;
        categoryRepository.Update(existing);
        await categoryRepository.SaveChangesAsync();

        cache.Remove(CategoriesCacheKey);
        cache.Remove(CategoryByIdKey + category.Id);
        cache.Remove(CategoryByNameKey + category.Name);

        return existing;
    }

    private void ValidateCategory(Category category, bool isNew)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Category name is required");
        if (!isNew && category.Id <= 0) throw new ArgumentException("Invalid category id");
    }
}

