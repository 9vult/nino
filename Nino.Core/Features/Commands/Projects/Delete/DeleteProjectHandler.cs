// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Delete;

public sealed class DeleteProjectHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<DeleteProjectHandler> logger
) : ICommandHandler<DeleteProjectCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(DeleteProjectCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Owner
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        logger.LogInformation("Deleting project {Project}", project);

        await db.Projects.Where(p => p.Id == command.ProjectId).ExecuteDeleteAsync();
        return Success();
    }
}
