using System.Net.Http.Json;

namespace ClientApp.Services;

public abstract class ApiServiceBase<TDto>(HttpClient http, string basePath) : IApiService<TDto>
{
    protected readonly HttpClient Http = http;
    protected readonly string BasePath = basePath;

    public virtual async Task<ApiResponse<IEnumerable<TDto>>> GetAllAsync()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<IEnumerable<TDto>>($"{BasePath}");
            return new ApiResponse<IEnumerable<TDto>>(true, res ?? []);
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<TDto>>(false, null, ex.Message);
        }
    }

    public virtual async Task<ApiResponse<TDto>> GetByIdAsync(int id)
    {
        try
        {
            var res = await Http.GetFromJsonAsync<TDto>($"{BasePath}/{id}");
            return res == null ? new ApiResponse<TDto>(false, default(TDto), "Not found") : new ApiResponse<TDto>(true, res);
        }
        catch (Exception ex)
        {
            return new ApiResponse<TDto>(false, default(TDto), ex.Message);
        }
    }

    public virtual async Task<ApiResponse<TDto>> CreateAsync(TDto dto)
    {
        try
        {
            var res = await Http.PostAsJsonAsync($"{BasePath}", dto);
            if (!res.IsSuccessStatusCode) return new ApiResponse<TDto>(false, default(TDto), res.ReasonPhrase);
            var created = await res.Content.ReadFromJsonAsync<TDto>();
            return new ApiResponse<TDto>(true, created ?? default(TDto));
        }
        catch (Exception ex)
        {
            return new ApiResponse<TDto>(false, default(TDto), ex.Message);
        }
    }

    public virtual async Task<ApiResponse<TDto>> UpdateAsync(TDto dto)
    {
        try
        {
            // Assuming TDto has an Id property is not guaranteed; child services can override to implement.
            throw new NotImplementedException("UpdateAsync should be overridden when needed to provide endpoint specifics.");
        }
        catch (Exception ex)
        {
            return new ApiResponse<TDto>(false, default(TDto), ex.Message);
        }
    }

    public virtual async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var res = await Http.DeleteAsync($"{BasePath}/{id}");
            return new ApiResponse<bool>(res.IsSuccessStatusCode, res.IsSuccessStatusCode, res.IsSuccessStatusCode ? null : res.ReasonPhrase);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>(false, false, ex.Message);
        }
    }
}
