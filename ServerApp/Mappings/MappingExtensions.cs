using Shared.DTOs;
using ServerApp.Domain;

namespace ServerApp.Mappings;

public static class MappingExtensions
{
    public static ProductDto ToDto(this Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name
        };
    }

    public static Product ToEntity(this ProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };
    }

    public static CategoryDto ToDto(this Category c)
    {
        return new CategoryDto { Id = c.Id, Name = c.Name };
    }

    public static Category ToEntity(this CategoryDto dto)
    {
        return new Category { Id = dto.Id, Name = dto.Name };
    }
}

