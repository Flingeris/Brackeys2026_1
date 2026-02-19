using System;
using System.Collections.Generic;
using System.Linq;

public static class GameUtils
{
    /// <summary>
    /// Returns a random element from the collection.
    /// Throws if the collection is null or empty.
    /// </summary>
    public static T GetRandomElement<T>(this IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        // Use indexable collections directly (arrays, lists, etc.)
        if (collection is IList<T> list && list.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        // Fallback for other IEnumerable<T> (HashSet, LINQ, etc.)
        var tempList = collection.ToList();
        if (tempList.Count == 0)
            throw new InvalidOperationException("Collection is empty.");

        int randomIndex = UnityEngine.Random.Range(0, tempList.Count);
        return tempList[randomIndex];
    }

    /// <summary>
    /// Shuffles the list in place using Fisher-Yates algorithm.
    /// </summary>
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Returns a new shuffled list from the collection.
    /// The original collection is not modified.
    /// </summary>
    public static List<T> GetShuffled<T>(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        var list = collection.ToList();
        Shuffle(list);
        return list;
    }

    public static T Pop<T>(this IList<T> list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        if (list.Count == 0)
            throw new InvalidOperationException("Collection is empty.");

        int lastIndex = list.Count - 1;
        T value = list[lastIndex];
        list.RemoveAt(lastIndex);
        return value;
    }
}