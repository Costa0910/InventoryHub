using System.Collections.Concurrent;

namespace ClientApp.Services;

public enum NotificationLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class NotificationItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Message { get; init; } = string.Empty;
    public string? Title { get; init; }
    public NotificationLevel Level { get; init; } = NotificationLevel.Info;
    public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;
    public int? AutoCloseMs { get; init; }
}

public class NotificationService
{
    // Thread-safe collection of notifications
    private readonly ConcurrentDictionary<string, NotificationItem> _items = new();

    public IEnumerable<NotificationItem> Items => _items.Values.OrderByDescending(i => i.Created);

    // notify UI
    public event Action? OnChange;

    public string Add(string message, string? title = null, NotificationLevel level = NotificationLevel.Info,
        int? autoCloseMs = 5000)
    {
        var item = new NotificationItem { Message = message, Title = title, Level = level, AutoCloseMs = autoCloseMs };
        _items[item.Id] = item;
        OnChange?.Invoke();

        if (autoCloseMs.HasValue && autoCloseMs.Value > 0) _ = AutoDismissAsync(item.Id, autoCloseMs.Value);

        return item.Id;
    }

    public bool Dismiss(string id)
    {
        var removed = _items.TryRemove(id, out _);
        if (removed) OnChange?.Invoke();
        return removed;
    }

    private async Task AutoDismissAsync(string id, int ms)
    {
        try
        {
            await Task.Delay(ms);
            Dismiss(id);
        }
        catch
        {
            // ignore
        }
    }

    public void ClearAll()
    {
        _items.Clear();
        OnChange?.Invoke();
    }
}