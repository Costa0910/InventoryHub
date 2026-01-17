namespace ClientApp.Services;

public record ApiResponse<T>(bool IsSuccess, T? Data = default, string? ErrorMessage = null);

