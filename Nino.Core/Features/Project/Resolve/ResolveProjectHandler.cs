// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.Project.Resolve;

public class ResolveProjectHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<ResolveProjectHandler> logger
)
{
    public async Task<Result<Guid>> HandleAsync(ResolveProjectQuery query)
    {
        var project = await db
            .Projects.Where(p =>
                (
                    p.GroupId == query.GroupId
                    || (query.IncludeObservers && p.Observers.Any(o => o.GroupId == query.GroupId))
                ) && (p.Nickname == query.Alias || p.Aliases.Any(a => a.Value == query.Alias))
            )
            .AsSingleQuery()
            .FirstOrDefaultAsync();

        if (project is null)
        {
            logger.LogTrace("Unable to resolve alias {Alias} to a project", query.Alias);
            return new Result<Guid>(ResultStatus.NotFound, Guid.Empty);
        }

        if (
            !project.IsPrivate
            || await verificationService.VerifyProjectPermissionsAsync(
                project.Id,
                query.RequestedBy,
                PermissionsLevel.Staff
            )
        )
        {
            logger.LogTrace(
                "Resolved alias \"{Alias}\" to project {Project}",
                query.Alias,
                project
            );
            return new Result<Guid>(ResultStatus.Success, project.Id);
        }

        // Unauthorized, pretend we do not see it
        return new Result<Guid>(ResultStatus.NotFound, Guid.Empty);
    }
}
