using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Domain;

public class Category
{
    public int Id { get; set; }
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Product> Products { get; set; } = [];
}
