using Microsoft.AspNetCore.Mvc;
using ServerApp.Application.Interfaces;
using ServerApp.Mappings;
using Shared.DTOs;

namespace ServerApp.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .WithSummary("Get categories (supports optional paging)");

        group.MapGet("/{id:int}", GetById)
            .WithName("GetCategoryById")
            .WithSummary("Get a category by id");

        group.MapPost("/", Create)
            .WithName("CreateCategory")
            .WithSummary("Create a new category");

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateCategory")
            .WithSummary("Update an existing category");

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteCategory")
            .WithSummary("Delete a category");

        return group;
    }

    private static async Task<IResult> GetCategories(ICategoryService service, int? pageNumber = null, int? pageSize = null, string? search = null)
    {
        if (!pageNumber.HasValue || !pageSize.HasValue)
        {
            var categories = (await service.GetAllAsync()).Select(c => c.ToDto()).ToArray();
            return TypedResults.Ok(categories);
        }

        var pn = Math.Max(1, pageNumber.Value);
        var ps = Math.Max(1, pageSize.Value);

        var (items, total) = await service.GetPagedAsync(pn, ps, search);
        var dtoItems = items.Select(c => c.ToDto());
        var paged = new PaginatedResponse<CategoryDto> { Items = dtoItems, TotalCount = total };
        return TypedResults.Ok(paged);
    }

    private static async Task<IResult> GetById(ICategoryService service, int id)
    {
        var c = await service.GetByIdAsync(id);
        return c is null ? TypedResults.NotFound() : TypedResults.Ok(c.ToDto());
    }

    private static async Task<IResult> Create(ICategoryService service, CategoryDto dto)
    {
        var created = await service.CreateAsync(dto.ToEntity());
        return TypedResults.Created($"/api/categories/{created.Id}", created.ToDto());
    }

    private static async Task<IResult> Update(ICategoryService service, int id, CategoryDto dto)
    {
        if (id != dto.Id) return TypedResults.BadRequest(new ValidationProblemDetails { Title = "Id mismatch" });
        var updated = await service.UpdateAsync(dto.ToEntity());
        return TypedResults.Ok(updated.ToDto());
    }

    private static async Task<IResult> Delete(ICategoryService service, int id)
    {
        await service.DeleteAsync(id);
        return TypedResults.Ok();
    }
}