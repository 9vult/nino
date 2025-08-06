namespace Nino.Utilities;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, int, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
    
    public static IEnumerable<TSource> ConcatIf<TSource>(this IEnumerable<TSource> source, bool condition, IEnumerable<TSource> predicate)
    {
        return condition ? source.Concat(predicate) : source;
    }
}