using Microsoft.AspNetCore.Mvc;
using ServerApp.Application.Interfaces;
using ServerApp.Mappings;
using Shared.DTOs;

namespace ServerApp.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", GetAllProducts)
            .WithName("GetProducts")
            .WithSummary("Get all products");

        group.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by id");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product");

        group.MapPut("/{id:int}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product");

        group.MapDelete("/{id:int}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");

        return group;
    }

    private static async Task<IResult> GetAllProducts(IProductService service)
    {
        var products = (await service.GetAllAsync()).Select(p => p.ToDto());
        return TypedResults.Ok(products);
    }

    private static async Task<IResult> GetProductById(IProductService service, int id)
    {
        var product = await service.GetByIdAsync(id);
        return product is null ? TypedResults.NotFound() : TypedResults.Ok(product.ToDto());
    }

    private static async Task<IResult> CreateProduct(IProductService service, ProductDto dto)
    {
        var entity = dto.ToEntity();
        var created = await service.CreateAsync(entity);
        return TypedResults.Created($"/api/products/{created.Id}", created.ToDto());
    }

    private static async Task<IResult> UpdateProduct(IProductService service, int id, ProductDto dto)
    {
        if (id != dto.Id) return TypedResults.BadRequest(new ValidationProblemDetails { Title = "Id mismatch" });
        var entity = dto.ToEntity();
        var updated = await service.UpdateAsync(entity);
        return TypedResults.Ok(updated.ToDto());
    }

    private static async Task<IResult> DeleteProduct(IProductService service, int id)
    {
        await service.DeleteAsync(id);
        return TypedResults.Ok();
    }
}