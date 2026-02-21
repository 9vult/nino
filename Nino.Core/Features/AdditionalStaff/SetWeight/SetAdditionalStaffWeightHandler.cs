// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.AdditionalStaff.SetWeight;

public sealed class SetAdditionalStaffWeightHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<SetAdditionalStaffWeightHandler> logger
)
{
    public async Task<Result> HandleAsync(SetAdditionalStaffWeightCommand action)
    {
        var (projectId, episodeNumber, abbreviation, weight, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result(ResultStatus.Unauthorized);

        var episode = await db
            .Episodes.Where(p => p.ProjectId == projectId)
            .SingleOrDefaultAsync(p => p.Number == episodeNumber);
        if (episode is null)
            return new Result(ResultStatus.NotFound);

        var staff = episode.AdditionalStaff.SingleOrDefault(s =>
            s.Role.Abbreviation == abbreviation
        );
        if (staff is null)
            return new Result(ResultStatus.NotFound);

        logger.LogInformation(
            "Setting weight of {Abbreviation} for {Episode} to {Weight}",
            abbreviation,
            episode,
            weight
        );

        staff.Role.Weight = weight;
        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
