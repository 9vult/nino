// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.Episodes.RejectAirEstimate;

public class RejectAirEstimateHandler(
    NinoDbContext db,
    IUserVerificationService verificationService
) : ICommandHandler<RejectAirEstimateCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RejectAirEstimateCommand command)
    {
        var episode = await db.Episodes.FirstOrDefaultAsync(e => e.Id == command.EpisodeId);

        if (episode is null)
            return Result.Fail(ResultStatus.EpisodeNotFound);

        var verification = await verificationService.VerifyProjectPermissionsAsync(
            episode.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Staff
        );
        if (!verification.IsSuccess)
            return Result.Fail(verification.Status);

        episode.AirNotificationStatus = AirNotificationStatus.EstimatedIncorrectly;
        await db.SaveChangesAsync();
        return Result.Success();
    }
}
