// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Release.Custom;

public sealed class ReleaseCustomHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<ReleaseCustomHandler> logger
) : ICommandHandler<ReleaseCustomCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> HandleAsync(ReleaseCustomCommand command)
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
            .Where(p => p.Id == command.ProjectId)
            .Select(p => new
            {
                p.Observers,
                p.DelegateObserverId,
                p.DelegateObserver,
            })
            .FirstOrDefaultAsync();
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        logger.LogInformation(
            "Publishing Release of project {ProjectId} \"{ReleaseLabel}\" to local group and {ObserverCount} observers",
            command.ProjectId,
            command.Label ?? string.Empty,
            project.Observers.Count
        );

        List<Task> tasks =
        [
            eventBus.PublishAsync(
                new CustomReleasedEvent(
                    ProjectId: command.ProjectId,
                    Label: command.Label,
                    Urls: command.Urls,
                    Publish: project.DelegateObserver is null,
                    PrimaryRoleId: command.PrimaryRoleId,
                    SecondaryRoleId: command.SecondaryRoleId,
                    TertiaryRoleId: command.TertiaryRoleId,
                    Commentary: command.Commentary
                )
            ),
            .. project.Observers.Select(observer =>
                eventBus.PublishAsync(
                    new CustomReleasedObserverEvent(
                        ProjectId: command.ProjectId,
                        ObserverId: observer.Id,
                        Label: command.Label,
                        Urls: command.Urls,
                        Commentary: command.Commentary,
                        Publish: observer.Id == project.DelegateObserverId
                    )
                )
            ),
        ];

        await Task.WhenAll(tasks);
        return Success();
    }
}
