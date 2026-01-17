using System.Net;
using System.Text.Json;

namespace ClientApp.Services;

public class ApiErrorHandler(NotificationService? notifications = null) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return response;

        // Try to read ProblemDetails or ValidationProblemDetails
        ProblemDetailsDto? pd = null;
        IDictionary<string, string[]>? errors = null;
        try
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(text))
            {
                // Try to parse as JSON; check for 'errors' property
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    errors = new Dictionary<string, string[]>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        var list = new List<string>();
                        foreach (var item in prop.Value.EnumerateArray()) list.Add(item.GetString() ?? string.Empty);
                        errors[prop.Name] = list.ToArray();
                    }
                }

                // try to bind to ProblemDetails for Title/Detail/Status
                try
                {
                    pd = JsonSerializer.Deserialize<ProblemDetailsDto>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    // ignore
                }
            }
        }
        catch
        {
            // ignore parse errors
        }

        var title = pd?.Title ?? response.ReasonPhrase;
        var detail = pd?.Detail;

        // Notify UI
        try
        {
            var msg = title ?? "An error occurred";
            if (errors is { Count: > 0 })
            {
                msg += ": " + string.Join("; ", errors.SelectMany(kv => kv.Value.Take(2)));
            }
            var level = response.StatusCode == HttpStatusCode.BadRequest ? NotificationLevel.Warning : NotificationLevel.Error;
            notifications?.Add(msg, title, level, 8000);
        }
        catch
        {
            // swallow notification errors
        }

        throw new ApiException((int)response.StatusCode, title, title, detail, errors);
    }
}
