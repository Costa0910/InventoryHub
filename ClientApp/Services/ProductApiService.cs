using System.Net.Http.Json;
using Shared.DTOs;

namespace ClientApp.Services;

public class ProductApiService(HttpClient http) : ApiServiceBase<ProductDto>(http, "/api/products")
{
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
}
