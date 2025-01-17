using System.Collections.ObjectModel;

namespace PicsyncClient.Models;

public class SelectItem<T>()
{
    public SelectItem(T item, bool isSeleced = false) : this()
    {
        Item       = item;
        IsSelected = isSeleced;
    }

    public required T    Item       { get; set; }
    public required bool IsSelected { get; set; }
}