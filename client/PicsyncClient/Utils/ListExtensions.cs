using System.Collections.ObjectModel;

namespace PicsyncClient.Utils;

public static class ListExtensions
{
    public static void ReplaceOrAdd<T>(this List<T> list, T oldItem, T newItem)
    {
        int index = list.IndexOf(oldItem);

        if (index != -1)
        {
            list.RemoveAt(index);
            list.Insert(index, newItem);
        }
        else
        {
            list.Add(newItem);
        }
    }
}
public static class ObservableCollectionExtensions
{
    public static void ReplaceOrAdd<T>(this ObservableCollection<T> list, T oldItem, T newItem)
    {
        int index = list.IndexOf(oldItem);

        if (index != -1)
        {
            list.RemoveAt(index);
            list.Insert(index, newItem);
        }
        else
        {
            list.Add(newItem);
        }
    }
}