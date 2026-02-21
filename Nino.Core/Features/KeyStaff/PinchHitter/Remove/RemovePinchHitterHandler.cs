// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.KeyStaff.PinchHitter.Remove;

public sealed class RemovePinchHitterHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<RemovePinchHitterHandler> logger
)
{
    public async Task<Result> HandleAsync(RemovePinchHitterCommand action)
    {
        var (projectId, episodeNumber, abbreviation, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result(ResultStatus.Unauthorized);

        var episode = await db
            .Episodes.Where(e => e.ProjectId == projectId)
            .SingleOrDefaultAsync(e => e.Number == episodeNumber);

        if (episode is null)
            return new Result(ResultStatus.NotFound);

        var pinchHitter = episode.PinchHitters.SingleOrDefault(h => h.Abbreviation == abbreviation);

        if (pinchHitter is null)
            return new Result(ResultStatus.NotFound);

        logger.LogInformation(
            "Removing pinch hitter {PinchHitter} from project {ProjectId} episode {Episode}",
            pinchHitter,
            projectId,
            episodeNumber
        );

        episode.PinchHitters.Remove(pinchHitter);
        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
