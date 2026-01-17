using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Web;
using Shared.DTOs;

namespace ClientApp.Services;

public class ProductApiService(HttpClient http) : ApiServiceBase<ProductDto>(http, "/api/products")
{
    // simple in-memory cache for paged product responses
    private readonly ConcurrentDictionary<string, (PaginatedResponse<ProductDto> Data, DateTime Timestamp)> _pagedCache
        = new();
    private readonly TimeSpan _cacheTtl = TimeSpan.FromSeconds(30);

    private string BuildCacheKey(int pageNumber, int pageSize, string? search, int? categoryId)
    {
        return $"p:{pageNumber}:s:{pageSize}:q:{search ?? string.Empty}:c:{categoryId?.ToString() ?? "0"}";
    }

    public override async Task<ApiResponse<ProductDto>> UpdateAsync(ProductDto dto)
    {
        try
        {
            var res = await Http.PutAsJsonAsync($"{BasePath}/{dto.Id}", dto);
            if (!res.IsSuccessStatusCode) return new ApiResponse<ProductDto>(false, null, res.ReasonPhrase);
            var updated = await res.Content.ReadFromJsonAsync<ProductDto>();
            return new ApiResponse<ProductDto>(true, updated);
        }
        catch (Exception ex)
        {
            return new ApiResponse<ProductDto>(false, null, ex.Message);
        }
    }

    public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? search = null, int? categoryId = null)
    {
        var key = BuildCacheKey(pageNumber, pageSize, search, categoryId);
        if (_pagedCache.TryGetValue(key, out var entry) && (DateTime.UtcNow - entry.Timestamp) < _cacheTtl)
        {
            return new ApiResponse<PaginatedResponse<ProductDto>>(true, entry.Data);
        }

        try
        {
            var qb = HttpUtility.ParseQueryString(string.Empty);
            qb["pageNumber"] = pageNumber.ToString();
            qb["pageSize"] = pageSize.ToString();
            if (!string.IsNullOrEmpty(search)) qb["search"] = search;
            if (categoryId.HasValue) qb["categoryId"] = categoryId.Value.ToString();

            var url = $"{BasePath}?{qb}";
            var data = await Http.GetFromJsonAsync<PaginatedResponse<ProductDto>>(url);
            var result = data ?? new PaginatedResponse<ProductDto>();
            _pagedCache[key] = (result, DateTime.UtcNow);
            return new ApiResponse<PaginatedResponse<ProductDto>>(true, result);
        }
        catch (ApiException aex)
        {
            return new ApiResponse<PaginatedResponse<ProductDto>>(false, null, aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaginatedResponse<ProductDto>>(false, null, ex.Message);
        }
    }

    // Cache invalidation helpers: clear paged cache when data mutates
    private void InvalidatePagedCacheForAll()
    {
        _pagedCache.Clear();
    }

    public override async Task<ApiResponse<ProductDto>> CreateAsync(ProductDto dto)
    {
        var res = await base.CreateAsync(dto);
        if (res.IsSuccess)
        {
            InvalidatePagedCacheForAll();
        }
        return res;
    }

    public override async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var res = await base.DeleteAsync(id);
        if (res.IsSuccess)
        {
            InvalidatePagedCacheForAll();
        }
        return res;
    }
}
