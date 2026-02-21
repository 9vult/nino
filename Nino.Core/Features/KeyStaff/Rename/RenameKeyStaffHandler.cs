// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.KeyStaff.Rename;

public sealed class RenameKeyStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<RenameKeyStaffHandler> logger
)
{
    public async Task<Result> HandleAsync(RenameKeyStaffCommand action)
    {
        var (projectId, oldAbbreviation, newAbbreviation, fullName, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == projectId);
        if (project is null)
            return new Result(ResultStatus.NotFound);

        var staff = project.KeyStaff.SingleOrDefault(s => s.Role.Abbreviation == oldAbbreviation);
        if (staff is null)
            return new Result(ResultStatus.NotFound);

        var taskAlreadyExists =
            project.KeyStaff.Any(s => s.Role.Abbreviation == newAbbreviation)
            || project
                .Episodes.SelectMany(e => e.AdditionalStaff)
                .Any(s => s.Role.Abbreviation == newAbbreviation);

        if (taskAlreadyExists)
            return new Result(ResultStatus.Conflict);

        logger.LogInformation(
            "Renaming {Project} Key Staff {KeyStaff} to {FullName} ({Abbreviation})",
            project,
            staff,
            fullName,
            newAbbreviation
        );

        staff.Role.Abbreviation = newAbbreviation;
        staff.Role.Name = fullName;

        foreach (var episode in project.Episodes)
        {
            episode
                .Tasks.SingleOrDefault(t => t.Abbreviation == oldAbbreviation)
                ?.Abbreviation = newAbbreviation;
        }

        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
