// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.KeyStaff.SetWeight;

public sealed class SetKeyStaffWeightHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<SetKeyStaffWeightHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(SetKeyStaffWeightCommand input)
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<GenericResponse>.Fail(ResultStatus.Unauthorized);

        var staff = await db.Staff.SingleAsync(s => s.Id == input.StaffId);

        logger.LogInformation(
            "Setting weight of Key Staff {Staff} to {Weight}",
            staff,
            input.NewWeight
        );

        staff.Role.Weight = input.NewWeight;
        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new GenericResponse(p.Title, p.Type, p.PosterUrl))
            .SingleAsync();

        return Result<GenericResponse>.Success(response);
    }
}
