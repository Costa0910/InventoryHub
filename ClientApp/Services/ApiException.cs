namespace ClientApp.Services;

public class ApiException(
    int statusCode,
    string? message = null,
    string? title = null,
    string? detail = null,
    IDictionary<string, string[]>? errors = null)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? Title { get; } = title;
    public string? Detail { get; } = detail;
    public IDictionary<string, string[]>? Errors { get; } = errors;
}
