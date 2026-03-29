// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.DelegateObserver.Remove;

public sealed class RemoveDelegateObserverHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveDelegateObserverHandler> logger
) : ICommandHandler<RemoveDelegateObserverCommand, Result>
{
    public async Task<Result> HandleAsync(RemoveDelegateObserverCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db
            .Projects.Include(p => p.Observers)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        logger.LogInformation("Removing Delegate Observer for project {ProjectId}", project.Id);

        project.DelegateObserverId = null;
        await db.SaveChangesAsync();
        return Success();
    }
}
