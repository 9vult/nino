// SPDX-License-Identifier: MPL-2.0

using NaturalSort.Extension;
using Nino.Domain.Entities;

namespace Nino.Core.Extensions;

public static class EpisodeEnumerableExtensions
{
    public static IEnumerable<Episode> OrderByNumber(this IEnumerable<Episode> episodes)
    {
        return episodes.OrderBy(
            e => e.Number.Value,
            StringComparer.OrdinalIgnoreCase.WithNaturalSort()
        );
    }
}
