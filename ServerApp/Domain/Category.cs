using System.ComponentModel.DataAnnotations;

namespace ServerApp.Domain;

public class Category
{
    public int Id { get; init; }
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; init; } = string.Empty;

    // Navigation
    public ICollection<Product> Products { get; init; } = [];
}

