using System.Net.Http.Json;
using System.Web;
using Shared.DTOs;

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
            return new ApiResponse<IEnumerable<TDto>>(true, res ?? Array.Empty<TDto>());
        }
        catch (ApiException aex)
        {
            return new ApiResponse<IEnumerable<TDto>>(false, null, aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<TDto>>(false, null, ex.Message);
        }
    }

    public virtual async Task<ApiResponse<PaginatedResponse<TDto>>> GetPagedAsync(int pageNumber, int pageSize, IDictionary<string, string?>? filters = null)
    {
        try
        {
            var qb = HttpUtility.ParseQueryString(string.Empty);
            qb["pageNumber"] = pageNumber.ToString();
            qb["pageSize"] = pageSize.ToString();
            if (filters != null)
            {
                foreach (var kv in filters)
                {
                    if (!string.IsNullOrEmpty(kv.Value)) qb[kv.Key] = kv.Value;
                }
            }

            var url = $"{BasePath}?{qb}";
            var res = await Http.GetFromJsonAsync<PaginatedResponse<TDto>>(url);
            return new ApiResponse<PaginatedResponse<TDto>>(true, res ?? new PaginatedResponse<TDto>());
        }
        catch (ApiException aex)
        {
            return new ApiResponse<PaginatedResponse<TDto>>(false, null, aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaginatedResponse<TDto>>(false, null, ex.Message);
        }
    }

    public virtual async Task<ApiResponse<TDto>> GetByIdAsync(int id)
    {
        try
        {
            var res = await Http.GetFromJsonAsync<TDto>($"{BasePath}/{id}");
            return res == null ? new ApiResponse<TDto>(false, default(TDto), "Not found") : new ApiResponse<TDto>(true, res);
        }
        catch (ApiException aex)
        {
            return new ApiResponse<TDto>(false, default(TDto), aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
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
        catch (ApiException aex)
        {
            return new ApiResponse<TDto>(false, default(TDto), aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
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
        catch (ApiException aex)
        {
            return new ApiResponse<TDto>(false, default(TDto), aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
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
        catch (ApiException aex)
        {
            return new ApiResponse<bool>(false, false, aex.Message) { StatusCode = aex.StatusCode, Errors = aex.Errors };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>(false, false, ex.Message);
        }
    }
}
