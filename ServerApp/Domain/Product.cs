using System.ComponentModel.DataAnnotations;

namespace ServerApp.Domain;

public class Product
{
    public int Id { get; init; }
    [StringLength(500, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    // Relationship
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

