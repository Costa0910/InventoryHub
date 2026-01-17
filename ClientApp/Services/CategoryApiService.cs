using System.Net.Http.Json;
using Shared.DTOs;

namespace ClientApp.Services;

public class CategoryApiService(HttpClient http) : ApiServiceBase<CategoryDto>(http, "/api/categories")
{
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
}
