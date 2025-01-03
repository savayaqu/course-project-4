using System.Collections.ObjectModel;

namespace PicsyncClient.Models;

public class ItemssGroup<T> : ObservableCollection<T>
{
    public string Title { get; set; }

    public ItemssGroup(string title, ObservableCollection<T> items) : base(items)
    {
        Title = title;
    }
}