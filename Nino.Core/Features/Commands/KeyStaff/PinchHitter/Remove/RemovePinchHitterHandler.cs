// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.KeyStaff.PinchHitter.Remove;

public sealed class RemovePinchHitterHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemovePinchHitterHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(RemovePinchHitterCommand input)
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<GenericResponse>.Fail(ResultStatus.Unauthorized);

        var episode = await db.Episodes.SingleAsync(e => e.Id == input.EpisodeId);
        var task = episode.Tasks.Single(t => t.Id == input.TaskId);

        var pinchHitter = episode.PinchHitters.SingleOrDefault(p =>
            p.Abbreviation == task.Abbreviation
        );

        if (pinchHitter is null)
            return Result<GenericResponse>.Fail(ResultStatus.NotFound);

        logger.LogInformation("Removing Pinch Hitter for {Episode}'s {Task}", episode, task);

        episode.PinchHitters.Remove(pinchHitter);
        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new GenericResponse(p.Title, p.Type, p.PosterUrl))
            .SingleAsync();

        return Result<GenericResponse>.Success(response);
    }
}
