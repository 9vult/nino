// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Utilities;

public static class CollectionExtensions
{
    public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        var count = 0;
        foreach (var item in collection.Where(predicate).ToList())
            count += collection.Remove(item) ? 1 : 0;
        return count;
    }
}
