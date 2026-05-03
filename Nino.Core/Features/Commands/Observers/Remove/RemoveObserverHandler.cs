// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Observers.Remove;

public sealed class RemoveObserverHandler(
    NinoDbContext db,
    IEventBus eventBus,
    IUserVerificationService verificationService,
    ILogger<RemoveObserverHandler> logger
) : ICommandHandler<RemoveObserverCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RemoveObserverCommand command)
    {
        var observer = await db.Observers.FirstOrDefaultAsync(o => o.Id == command.ObserverId);

        if (observer is null)
            return Fail(ResultStatus.ObserverNotFound);

        // Proceed only if the user a) made the observer or b) is an admin in the observing group
        var verification = await verificationService.VerifyGroupPermissionsAsync(
            observer.GroupId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (
            observer.OwnerId != command.RequestedBy
            && !verification.IsSuccess
            && !command.OverrideVerification
        )
            return Fail(verification.Status);

        // Check if the deleted observer was a delegate; if so, we want to notify the project
        var delegatedProject = await db.Projects.FirstOrDefaultAsync(p =>
            p.DelegateObserverId == command.ObserverId
        );

        if (delegatedProject is not null)
        {
            delegatedProject.DelegateObserverId = null;
            await eventBus.PublishAsync(
                new DelegateObserverDeletedEvent(delegatedProject.Id, observer.GroupId)
            );
            await db.SaveChangesAsync();
        }

        logger.LogInformation(
            "Deleting group {GroupId}'s observer of project {ProjectId}",
            observer.GroupId,
            observer.ProjectId
        );

        await db.Observers.Where(o => o.Id == command.ObserverId).ExecuteDeleteAsync();
        return Success();
    }
}
