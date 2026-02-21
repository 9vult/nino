// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.KeyStaff.PinchHitter.Set;

public sealed class SetPinchHitterHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<SetPinchHitterHandler> logger
)
{
    public async Task<Result> HandleAsync(SetPinchHitterCommand action)
    {
        var (projectId, episodeNumber, abbreviation, userId, requestedBy) = action;

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

        var keyStaff =
            await db
                .Projects.Where(p => p.Id == projectId)
                .Select(p => p.KeyStaff)
                .SingleOrDefaultAsync() ?? [];

        if (episode is null || keyStaff.All(s => s.Role.Abbreviation != abbreviation))
            return new Result(ResultStatus.NotFound);

        logger.LogInformation(
            "Setting a pinch hitter for task {Abbreviation} for {ProjectId} episode {Episode}",
            abbreviation,
            projectId,
            episodeNumber
        );

        var pinchHitter = episode.PinchHitters.SingleOrDefault(h => h.Abbreviation == abbreviation);
        if (pinchHitter is not null)
        {
            pinchHitter.UserId = userId;
            await db.SaveChangesAsync();
            return new Result(ResultStatus.Success);
        }

        pinchHitter = new Entities.PinchHitter { UserId = userId, Abbreviation = abbreviation };
        episode.PinchHitters.Add(pinchHitter);

        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
