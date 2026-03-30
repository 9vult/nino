// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Groups.Admins.Add;

public sealed class AddGroupAdminHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddGroupAdminHandler> logger
) : ICommandHandler<AddGroupAdminCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddGroupAdminCommand command)
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

        if (config.Administrators.Any(a => a.UserId == command.UserId))
            return Fail(ResultStatus.BadRequest);

        logger.LogInformation(
            "Adding {UserId} as an administrator for group {GroupId}",
            command.UserId,
            group.Id
        );

        config.Administrators.Add(new Administrator { UserId = command.UserId });
        await db.SaveChangesAsync();
        return Success();
    }
}
