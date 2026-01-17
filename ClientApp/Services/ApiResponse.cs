namespace ClientApp.Services;

public record ApiResponse<T>(bool IsSuccess, T? Data = default, string? ErrorMessage = null)
{
    public int? StatusCode { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
}
