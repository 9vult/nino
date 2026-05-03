// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Admins.Remove;

public sealed class RemoveProjectAdminHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveProjectAdminHandler> logger
) : ICommandHandler<RemoveProjectAdminCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RemoveProjectAdminCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var admin = project.Administrators.FirstOrDefault(a => a.UserId == command.UserId);
        if (admin is null)
            return Fail(ResultStatus.BadRequest);

        logger.LogInformation(
            "Removing {UserId} as an administrator from project {ProjectId}",
            command.UserId,
            project.Id
        );

        project.Administrators.Remove(admin);
        await db.SaveChangesAsync();
        return Success();
    }
}
