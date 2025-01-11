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