namespace Nino.Utilities;

public static class CollectionExtensions
{
    public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            var element = collection.ElementAt(i);
            if (!predicate(element))
                continue;
            collection.Remove(element);
            i--;
        }
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newItems)
    {
        foreach (var item in newItems)
        {
            collection.Add(item);
        }
    }
}
