// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.Episodes.Roster;

public partial class EpisodeRosterHandler(
    DataContext db,
    IDataService dataService,
    IUserVerificationService verificationService,
    ILogger<EpisodeRosterHandler> logger
)
{
    public async Task<Result<EpisodeStatusDto>> HandleAsync(EpisodeRosterCommand action)
    {
        var (projectId, episodeNumber, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Staff
            )
        )
            return new Result<EpisodeStatusDto>(ResultStatus.Unauthorized);

        var episode = await db.Episodes.SingleOrDefaultAsync(e =>
            e.ProjectId == projectId && e.Number == episodeNumber
        );
        if (episode is null)
            return new Result<EpisodeStatusDto>(ResultStatus.NotFound);

        logger.LogInformation("Getting roster status for {Episode}", episode);

        return new Result<EpisodeStatusDto>(
            ResultStatus.Success,
            await dataService.GetEpisodeStatusAsync(projectId, episodeNumber)
        );
    }
}
