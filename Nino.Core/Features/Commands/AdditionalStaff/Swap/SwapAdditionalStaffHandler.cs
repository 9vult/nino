// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.AdditionalStaff.Swap;

public sealed class SwapAdditionalStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<SwapAdditionalStaffHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(SwapAdditionalStaffCommand input)
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
            "Swapping user {UserId} in for Additional Staff {Staff}",
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
