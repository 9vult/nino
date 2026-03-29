// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.DelegateObserver.Set;

public sealed class SetDelegateObserverHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<SetDelegateObserverHandler> logger
) : ICommandHandler<SetDelegateObserverCommand, Result>
{
    public async Task<Result> HandleAsync(SetDelegateObserverCommand command)
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

        var observer = project.Observers.FirstOrDefault(o => o.Id == command.ObserverId);
        if (observer is null)
            return Fail(ResultStatus.ObserverNotFound);

        logger.LogInformation(
            "Setting observer {ObserverId} as Delegate Observer for project {ProjectId}",
            observer.Id,
            project.Id
        );

        project.DelegateObserverId = command.ObserverId;
        await db.SaveChangesAsync();
        return Success();
    }
}
