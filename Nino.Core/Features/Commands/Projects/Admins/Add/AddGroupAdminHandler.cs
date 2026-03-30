// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Admins.Add;

public sealed class AddProjectAdminHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddProjectAdminHandler> logger
) : ICommandHandler<AddProjectAdminCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddProjectAdminCommand command)
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

        if (project.Administrators.Any(a => a.UserId == command.UserId))
            return Fail(ResultStatus.BadRequest);

        logger.LogInformation(
            "Adding {UserId} as an administrator for project {ProjectId}",
            command.UserId,
            project.Id
        );

        project.Administrators.Add(new Administrator { UserId = command.UserId });
        await db.SaveChangesAsync();
        return Success();
    }
}
