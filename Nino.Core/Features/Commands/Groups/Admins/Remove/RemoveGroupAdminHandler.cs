// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Groups.Admins.Remove;

public sealed class RemoveGroupAdminHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveGroupAdminHandler> logger
) : ICommandHandler<RemoveGroupAdminCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RemoveGroupAdminCommand command)
    {
        var verification = await verificationService.VerifyGroupPermissionsAsync(
            command.GroupId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!command.OverrideVerification && !verification.IsSuccess)
            return Fail(verification.Status);

        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == command.GroupId);
        if (group is null)
            return Fail(ResultStatus.GroupNotFound);
        var config = group.Configuration;

        var admin = config.Administrators.FirstOrDefault(a => a.UserId == command.UserId);
        if (admin is null)
            return Fail(ResultStatus.BadRequest);

        logger.LogInformation(
            "Removing {UserId} as an administrator from group {GroupId}",
            command.UserId,
            group.Id
        );

        config.Administrators.Remove(admin);
        await db.SaveChangesAsync();
        return Success();
    }
}
