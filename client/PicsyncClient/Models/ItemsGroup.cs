using System.Collections.ObjectModel;

namespace PicsyncClient.Models;

public class ItemsGroup<T> : ObservableCollection<T>
{
    public string Title { get; set; }

    public ItemsGroup(string title, ObservableCollection<T> items) : base(items)
    {
        Title = title;
    }
}

public class ItemsKeyGroup<TKey, TItem> : ObservableCollection<TItem>
{
    public TKey Key{ get; set; }

    public ItemsKeyGroup(TKey key, ObservableCollection<TItem> items) : base(items)
    {
        Key = key;
    }
}