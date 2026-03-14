// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.AdditionalStaff.Rename;

public sealed class RenameAdditionalStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RenameAdditionalStaffHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(RenameAdditionalStaffCommand input)
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

        var staff = episode.AdditionalStaff.Single(s => s.Id == input.StaffId);
        var task = episode.Tasks.Single(t => t.Abbreviation == staff.Role.Abbreviation);

        if (
            staff.Role.Abbreviation != input.NewAbbreviation
            && episode.Tasks.Any(t => t.Abbreviation == input.NewAbbreviation)
        )
            return Result<GenericResponse>.Fail(ResultStatus.Conflict);

        logger.LogInformation(
            "Renaming {Episode}'s Additional Staff {Staff} to {Name} ({Abbreviation})",
            episode,
            staff,
            input.NewName,
            input.NewAbbreviation
        );

        staff.Role.Abbreviation = input.NewAbbreviation;
        staff.Role.Name = input.NewName;
        task.Abbreviation = input.NewAbbreviation;

        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new GenericResponse(p.Title, p.Type, p.PosterUrl))
            .SingleAsync();

        return Result<GenericResponse>.Success(response);
    }
}
