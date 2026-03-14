// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.KeyStaff.PinchHitter.Set;

public sealed class SetPinchHitterHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<SetPinchHitterHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(SetPinchHitterCommand input)
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

        logger.LogInformation(
            "Setting user {User} as Pinch Hitter for {Episode}'s {Task}",
            input.MemberId,
            episode,
            task
        );

        var pinchHitter = episode.PinchHitters.SingleOrDefault(p =>
            p.Abbreviation == task.Abbreviation
        );

        if (pinchHitter is not null)
        {
            pinchHitter.UserId = input.MemberId;
        }
        else
        {
            pinchHitter = new() { UserId = input.MemberId, Abbreviation = task.Abbreviation };
            episode.PinchHitters.Add(pinchHitter);
        }

        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new GenericResponse(p.Title, p.Type, p.PosterUrl))
            .SingleAsync();

        return Result<GenericResponse>.Success(response);
    }
}
