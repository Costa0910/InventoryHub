using ServerApp.Application.Interfaces;
using ServerApp.Domain;

namespace ServerApp.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<Category> CreateAsync(Category category)
    {
        ValidateCategory(category, isNew: true);

        // Prevent duplicate names
        var existing = await categoryRepository.GetByNameAsync(category.Name);
        if (existing != null)
            throw new InvalidOperationException("Category with the same name already exists.");

        await categoryRepository.AddAsync(category);
        await categoryRepository.SaveChangesAsync();
        return category;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await categoryRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Category not found");

        categoryRepository.Delete(existing);
        await categoryRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await categoryRepository.GetAllAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await categoryRepository.GetByIdAsync(id);
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await categoryRepository.GetByNameAsync(name);
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
        return existing;
    }

    private void ValidateCategory(Category category, bool isNew)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Category name is required");
        if (!isNew && category.Id <= 0) throw new ArgumentException("Invalid category id");
    }
}

