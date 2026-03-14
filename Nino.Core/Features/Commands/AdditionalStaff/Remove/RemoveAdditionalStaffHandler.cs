// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.AdditionalStaff.Remove;

public sealed class RemoveAdditionalStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveAdditionalStaffHandler> logger
)
{
    public async Task<Result<RemoveAdditionalStaffResponse>> HandleAsync(
        RemoveAdditionalStaffCommand input
    )
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<RemoveAdditionalStaffResponse>.Fail(ResultStatus.Unauthorized);

        var episode = await db.Episodes.SingleAsync(ep => ep.Id == input.EpisodeId);
        var staff = episode.AdditionalStaff.Single(s => s.Id == input.StaffId);
        var task = episode.Tasks.Single(t => t.Abbreviation == staff.Role.Abbreviation);

        logger.LogInformation("Removing Additional Staff {Staff} from {Episode}", staff, episode);

        episode.AdditionalStaff.Remove(staff);
        episode.Tasks.Remove(task);

        episode.IsDone = episode.Tasks.All(t => t.IsDone);

        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new RemoveAdditionalStaffResponse(
                p.Title,
                p.Type,
                p.PosterUrl,
                IsEpisodeComplete: episode.IsDone
            ))
            .SingleAsync();

        return Result<RemoveAdditionalStaffResponse>.Success(response);
    }
}
