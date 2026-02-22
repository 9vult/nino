// SPDX-License-Identifier: MPL-2.0

using NaturalSort.Extension;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.Episodes.Remove;

public partial class RemoveEpisodeHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveEpisodeHandler> logger
)
{
    public async Task<Result<int>> HandleAsync(RemoveEpisodeCommand action)
    {
        var (projectId, firstEpisodeNumber, lastEpisodeNumber, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result<int>(ResultStatus.Unauthorized);

        var episodes = (await db.Episodes.Where(p => p.Id == projectId).ToListAsync())
            .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Number == firstEpisodeNumber);
        var lastIdx = episodes.FindIndex(e => e.Number == lastEpisodeNumber);

        if (firstIdx is -1 || lastIdx is -1)
            return new Result<int>(ResultStatus.NotFound);

        lastIdx += 1; // Exclusive range end

        logger.LogInformation(
            "Deleting episodes between {FirstEpisode} and {LastEpisode}",
            firstEpisodeNumber,
            lastEpisodeNumber
        );

        var deletable = episodes[firstIdx..lastIdx].Select(e => e.Id).ToList();
        await db.Episodes.Where(e => deletable.Contains(e.Id)).ExecuteDeleteAsync();

        return new Result<int>(ResultStatus.Success, deletable.Count);
    }
}
