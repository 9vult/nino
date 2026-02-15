// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Actions.Project.Resolve;

public class ProjectResolveHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<ProjectResolveHandler> logger
)
{
    public async Task<ProjectResolveResult> HandleAsync(ProjectResolveAction action)
    {
        var project = await db
            .Projects.Where(p =>
                (
                    p.GroupId == action.GroupId
                    || (
                        action.IncludeObservers && p.Observers.Any(o => o.GroupId == action.GroupId)
                    )
                ) && (p.Nickname == action.Alias || p.Aliases.Any(a => a.Value == action.Alias))
            )
            .FirstOrDefaultAsync();

        if (project is null)
        {
            logger.LogTrace("Unable to resolve alias {Alias} to a project", action.Alias);
            return new ProjectResolveResult(ActionStatus.NotFound, null);
        }

        if (
            !project.IsPrivate
            || await verificationService.VerifyProjectPermissionsAsync(
                project.Id,
                action.RequestedBy,
                PermissionsLevel.Staff
            )
        )
        {
            logger.LogTrace(
                "Resolved alias \"{Alias}\" to project {Project}",
                action.Alias,
                project
            );
            return new ProjectResolveResult(ActionStatus.Success, project.Id);
        }

        // Unauthorized, pretend we do not see it
        return new ProjectResolveResult(ActionStatus.NotFound, null);
    }
}
