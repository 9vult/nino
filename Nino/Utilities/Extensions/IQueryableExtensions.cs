using System.Linq.Expressions;

namespace Nino.Utilities.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(
        this IQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return condition ? source.Where(predicate) : source;
    }
}
