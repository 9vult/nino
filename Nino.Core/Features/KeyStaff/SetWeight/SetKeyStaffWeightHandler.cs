// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.KeyStaff.SetWeight;

public sealed class SetKeyStaffWeightHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<SetKeyStaffWeightCommand> logger
)
{
    public async Task<Result> HandleAsync(SetKeyStaffWeightCommand action)
    {
        var (projectId, abbreviation, weight, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result(ResultStatus.Unauthorized);

        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
        if (project is null)
            return new Result(ResultStatus.NotFound);

        var staff = project.KeyStaff.SingleOrDefault(s => s.Role.Abbreviation == abbreviation);
        if (staff is null)
            return new Result(ResultStatus.NotFound);

        logger.LogInformation(
            "Setting weight of {Abbreviation} for {Project} to {Weight}",
            abbreviation,
            project,
            weight
        );

        staff.Role.Weight = weight;
        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
