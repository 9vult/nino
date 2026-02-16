// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Actions.Project.Resolve;

public class ProjectResolveHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<ProjectResolveHandler> logger
)
{
    public async Task<Result<Guid>> HandleAsync(ProjectResolveAction action)
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
            return new Result<Guid>(ResultStatus.NotFound, Guid.Empty);
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
            return new Result<Guid>(ResultStatus.Success, project.Id);
        }

        // Unauthorized, pretend we do not see it
        return new Result<Guid>(ResultStatus.NotFound, Guid.Empty);
    }
}
