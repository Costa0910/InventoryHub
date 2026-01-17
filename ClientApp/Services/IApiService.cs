namespace ClientApp.Services;

public interface IApiService<TDto>
{
    Task<ApiResponse<IEnumerable<TDto>>> GetAllAsync();
    Task<ApiResponse<TDto>> GetByIdAsync(int id);
    Task<ApiResponse<TDto>> CreateAsync(TDto dto);
    Task<ApiResponse<TDto>> UpdateAsync(TDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

