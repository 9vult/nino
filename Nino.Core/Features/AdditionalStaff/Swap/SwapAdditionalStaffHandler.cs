// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.AdditionalStaff.Swap;

public sealed class SwapAdditionalStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<SwapAdditionalStaffHandler> logger
)
{
    public async Task<Result> HandleAsync(SwapAdditionalStaffCommand action)
    {
        var (projectId, episodeNumber, userId, abbreviation, requestedBy) = action;

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
            "Swapping {UserId} in to {Episode} for {Abbreviation}",
            userId,
            episode,
            abbreviation
        );

        staff.UserId = userId;
        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
