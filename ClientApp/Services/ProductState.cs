using Shared.DTOs;

namespace ClientApp.Services;

public class ProductState
{
    private readonly List<ProductDto> _items = [];

    public IReadOnlyList<ProductDto> Items => _items;

    public event Action? OnChange;

    public void SetAll(IEnumerable<ProductDto> items)
    {
        _items.Clear();
        _items.AddRange(items);
        OnChange?.Invoke();
    }

    public void AddOrUpdate(ProductDto item)
    {
        var idx = _items.FindIndex(x => x.Id == item.Id);
        if (idx >= 0)
        {
            _items[idx] = item;
        }
        else
        {
            // insert at top
            _items.Insert(0, item);
        }
        OnChange?.Invoke();
    }

    public void Remove(int id)
    {
        var idx = _items.FindIndex(x => x.Id == id);
        if (idx < 0) return;
        _items.RemoveAt(idx);
        OnChange?.Invoke();
    }
}

