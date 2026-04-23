// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using static Nino.Core.Features.Result<bool>;

namespace Nino.Core.Features.Queries.Episodes.ValidateRelease;

/// <summary>
/// Validates that all episodes to be released are complete
/// </summary>
/// <remarks>Returns <see langword="true"/> if any episodes aren't found</remarks>
public class ValidateReleaseHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<ValidateReleaseQuery, Result<bool>>
{
    /// <inheritdoc />
    public async Task<Result<bool>> HandleAsync(ValidateReleaseQuery query)
    {
        var episodes = (
            await db
                .Episodes.Where(e => e.ProjectId == query.ProjectId)
                .Select(e => new { e.Number, e.IsDone })
                .ToListAsync()
        )
            .OrderBy(e => e.Number.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Number == query.FirstEpisode);
        var lastIdx = episodes.FindIndex(e => e.Number == query.LastEpisode) + 1;

        // Pass if not found
        if (firstIdx < 0 || lastIdx < 1)
            return Success(true);

        return Success(episodes[firstIdx..lastIdx].All(e => e.IsDone));
    }
}
