// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.KeyStaff.Rename;

public sealed class RenameKeyStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RenameKeyStaffHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(RenameKeyStaffCommand input)
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<GenericResponse>.Fail(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .Where(p => p.Id == input.ProjectId)
            .SingleAsync();

        var staff = project.KeyStaff.Single(s => s.Id == input.StaffId);

        if (
            staff.Role.Abbreviation != input.NewAbbreviation
            && project
                .Episodes.SelectMany(e => e.Tasks)
                .Any(t => t.Abbreviation == input.NewAbbreviation)
        )
            return Result<GenericResponse>.Fail(ResultStatus.Conflict);

        logger.LogInformation(
            "Renaming {Project}'s Key Staff {Staff} to {Name} ({Abbreviation})",
            project,
            staff,
            input.NewName,
            input.NewAbbreviation
        );

        staff.Role.Abbreviation = input.NewAbbreviation;
        staff.Role.Name = input.NewName;

        foreach (var episode in project.Episodes)
        {
            var task = episode.Tasks.Single(t => t.Abbreviation == staff.Role.Abbreviation);
            task.Abbreviation = input.NewAbbreviation;
        }

        await db.SaveChangesAsync();

        var response = new GenericResponse(project.Title, project.Type, project.PosterUrl);
        return Result<GenericResponse>.Success(response);
    }
}
