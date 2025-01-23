using System.Diagnostics.CodeAnalysis;

namespace PicsyncClient.Models;

public class SelectItem<T>()
{
    [SetsRequiredMembers]
    public SelectItem(T item, bool isSeleced = false) : this()
    {
        Item       = item;
        IsSelected = isSeleced;
    }

    public required T Item { get; set; }
    public bool IsSelected { get; set; } = false;
}