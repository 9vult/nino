// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.AdditionalStaff.Rename;

public sealed class RenameAdditionalStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<RenameAdditionalStaffHandler> logger
)
{
    public async Task<Result> HandleAsync(RenameAdditionalStaffCommand action)
    {
        var (projectId, episodeNumber, oldAbbreviation, newAbbreviation, fullName, requestedBy) =
            action;

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
            s.Role.Abbreviation == oldAbbreviation
        );
        if (staff is null)
            return new Result(ResultStatus.NotFound);

        var taskAlreadyExists = episode.Tasks.Any(s => s.Abbreviation == newAbbreviation);
        if (taskAlreadyExists)
            return new Result(ResultStatus.Conflict);

        logger.LogInformation(
            "Renaming {Episode} Additional Staff {KeyStaff} to {FullName} ({Abbreviation})",
            episode,
            staff,
            fullName,
            newAbbreviation
        );

        staff.Role.Abbreviation = newAbbreviation;
        staff.Role.Name = fullName;
        episode
            .Tasks.FirstOrDefault(t => t.Abbreviation == oldAbbreviation)
            ?.Abbreviation = newAbbreviation;

        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
