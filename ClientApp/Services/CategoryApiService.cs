using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Web;
using Shared.DTOs;

namespace ClientApp.Services;

public class CategoryApiService(HttpClient http) : ApiServiceBase<CategoryDto>(http, "/api/categories")
{
    private readonly ConcurrentDictionary<string, (PaginatedResponse<CategoryDto> Data, DateTime Timestamp)> _pagedCache
        = new();
    private readonly TimeSpan _cacheTtl = TimeSpan.FromSeconds(30);

    private string BuildCacheKey(int pageNumber, int pageSize, string? search)
    {
        return $"p:{pageNumber}:s:{pageSize}:q:{search ?? string.Empty}";
    }

    public override async Task<ApiResponse<CategoryDto>> UpdateAsync(CategoryDto dto)
    {
        try
        {
            var res = await Http.PutAsJsonAsync($"{BasePath}/{dto.Id}", dto);
            if (!res.IsSuccessStatusCode) return new ApiResponse<CategoryDto>(false, null, res.ReasonPhrase);
            var updated = await res.Content.ReadFromJsonAsync<CategoryDto>();
            return new ApiResponse<CategoryDto>(true, updated);
        }
        catch (Exception ex)
        {
            return new ApiResponse<CategoryDto>(false, null, ex.Message);
        }
    }

    public async Task<ApiResponse<PaginatedResponse<CategoryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 20, string? search = null)
    {
        var key = BuildCacheKey(pageNumber, pageSize, search);
        if (_pagedCache.TryGetValue(key, out var entry) && (DateTime.UtcNow - entry.Timestamp) < _cacheTtl)
        {
            return new ApiResponse<PaginatedResponse<CategoryDto>>(true, entry.Data);
        }

        try
        {
            var qb = HttpUtility.ParseQueryString(string.Empty);
            qb["pageNumber"] = pageNumber.ToString();
            qb["pageSize"] = pageSize.ToString();
            if (!string.IsNullOrEmpty(search)) qb["search"] = search;

            var url = $"{BasePath}?{qb}";
            var data = await Http.GetFromJsonAsync<PaginatedResponse<CategoryDto>>(url);
            var result = data ?? new PaginatedResponse<CategoryDto>();
            _pagedCache[key] = (result, DateTime.UtcNow);
            return new ApiResponse<PaginatedResponse<CategoryDto>>(true, result);
        }
        catch (ApiException aex)
        {
            return new ApiResponse<PaginatedResponse<CategoryDto>>(false, null, aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaginatedResponse<CategoryDto>>(false, null, ex.Message);
        }
    }

    private void InvalidatePagedCacheForAll()
    {
        _pagedCache.Clear();
    }

    public override async Task<ApiResponse<CategoryDto>> CreateAsync(CategoryDto dto)
    {
        var res = await base.CreateAsync(dto);
        if (res.IsSuccess) InvalidatePagedCacheForAll();
        return res;
    }

    public override async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var res = await base.DeleteAsync(id);
        if (res.IsSuccess) InvalidatePagedCacheForAll();
        return res;
    }
}
