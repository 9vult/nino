// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;
using Nino.Core.Utilities;

namespace Nino.Core.Features.AdditionalStaff.Remove;

public sealed class RemoveAdditionalStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveAdditionalStaffHandler> logger
)
{
    public async Task<Result<bool>> HandleAsync(RemoveAdditionalStaffCommand action)
    {
        var (projectId, episodeNumber, abbreviation, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result<bool>(ResultStatus.Unauthorized);

        var episode = await db
            .Episodes.Where(p => p.ProjectId == projectId)
            .SingleOrDefaultAsync(p => p.Number == episodeNumber);
        if (episode is null)
            return new Result<bool>(ResultStatus.NotFound);

        var staff = episode.AdditionalStaff.SingleOrDefault(t =>
            t.Role.Abbreviation == abbreviation
        );
        if (staff is null)
            return new Result<bool>(ResultStatus.Conflict);

        logger.LogInformation("Removing Additional Staff {Staff} from {Episode}", staff, episode);

        episode.AdditionalStaff.Remove(staff);
        episode.Tasks.RemoveAll(t => t.Abbreviation == abbreviation);
        episode.IsDone = episode.Tasks.All(t => t.IsDone);

        await db.SaveChangesAsync();
        return new Result<bool>(ResultStatus.Success, episode.IsDone);
    }
}
