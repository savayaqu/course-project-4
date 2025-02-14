using CommunityToolkit.Mvvm.Collections;

namespace PicsyncClient.Utils;

public static class IListExtensions
{
    public static void ReplaceOrAdd<T>(this IList<T> list, T oldItem, T newItem)
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

    public static void UpdateFrom<T>(this IList<T> targetList, IList<T> fromList)
    {
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (!fromList.Contains(targetList[i]))
            {
                targetList.RemoveAt(i);
            }
        }

        foreach (var item in fromList)
        {
            if (!targetList.Contains(item))
            {
                targetList.Add(item);
            }
        }
    }
}

public static class ObservableGroupedCollectionExtensions
{
    public static void SyncWith<TKey, TElement>(
        this ObservableGroupedCollection<TKey, TElement> list1,
        ObservableGroupedCollection<TKey, TElement> list2)
        where TKey : notnull
    {
        // Удаляем группы из list1, которых нет в list2
        for (int i = list1.Count - 1; i >= 0; i--)
        {
            var group1 = list1[i];
            bool groupExistsInList2 = false;

            // Проверяем, есть ли группа с таким ключом в list2
            foreach (var group2 in list2)
            {
                if (EqualityComparer<TKey>.Default.Equals(group2.Key, group1.Key))
                {
                    groupExistsInList2 = true;
                    break;
                }
            }

            if (!groupExistsInList2)
            {
                list1.RemoveAt(i);
            }
        }

        // Синхронизируем группы
        foreach (var group2 in list2)
        {
            ObservableGroup<TKey, TElement>? group1 = null;

            // Ищем группу с таким же ключом в list1
            foreach (var g in list1)
            {
                if (EqualityComparer<TKey>.Default.Equals(g.Key, group2.Key))
                {
                    group1 = g;
                    break;
                }
            }

            if (group1 == null)
            {
                // Если группы с таким ключом нет в list1, добавляем новую группу
                list1.Add(new ObservableGroup<TKey, TElement>(group2.Key, group2.ToList()));
            }
            else
            {
                // Синхронизируем элементы внутри группы
                SyncElements(group1, group2);
            }
        }
    }

    private static void SyncElements<TKey, TElement>(
        ObservableGroup<TKey, TElement> group1,
        ObservableGroup<TKey, TElement> group2)
        where TKey : notnull
    {
        // Удаляем элементы из group1, которых нет в group2
        for (int i = group1.Count - 1; i >= 0; i--)
        {
            if (!group2.Contains(group1[i]))
            {
                group1.RemoveAt(i);
            }
        }

        // Добавляем в group1 элементы из group2, которых нет в group1
        foreach (var item in group2)
        {
            if (!group1.Contains(item))
            {
                group1.Add(item);
            }
        }
    }
}