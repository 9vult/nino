// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.KeyStaff.Swap;

public sealed class SwapKeyStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<SwapKeyStaffHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(SwapKeyStaffCommand input)
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
            "Swapping user {UserId} in for Key Staff {Staff}",
            input.MemberId,
            staff
        );

        staff.UserId = input.MemberId;
        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new GenericResponse(p.Title, p.Type, p.PosterUrl))
            .SingleAsync();

        return Result<GenericResponse>.Success(response);
    }
}
