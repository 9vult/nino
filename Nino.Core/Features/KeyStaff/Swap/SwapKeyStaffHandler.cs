// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.Add;
using Nino.Core.Services;

namespace Nino.Core.Features.KeyStaff.Swap;

public sealed class SwapKeyStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<AddKeyStaffHandler> logger
)
{
    public async Task<Result> HandleAsync(SwapKeyStaffCommand action)
    {
        var (projectId, userId, abbreviation, requestedBy) = action;

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
            "Swapping {UserId} in to {Project} for {Abbreviation}",
            userId,
            project,
            abbreviation
        );

        staff.UserId = userId;
        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
