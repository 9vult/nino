// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Project.Resolve;

public sealed class ResolveProjectHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<ResolveProjectHandler> logger
)
{
    public async Task<Result<ProjectId>> HandleAsync(ResolveProjectQuery query)
    {
        var (alias, groupId, requestedBy, includeObservers) = query;
        var project = await db
            .Projects.Where(p =>
                (
                    p.GroupId == groupId
                    || (includeObservers && p.Observers.Any(o => o.GroupId == groupId))
                ) && (p.Nickname == alias || p.Aliases.Any(a => a.Value == alias))
            )
            .Select(p => new { p.Id, p.IsPrivate })
            .AsSingleQuery()
            .SingleOrDefaultAsync();

        if (project is null)
        {
            logger.LogTrace("Unable to resolve alias {Alias} to a project", alias);
            return Result<ProjectId>.Fail(ResultStatus.ProjectNotFound);
        }

        if (!project.IsPrivate)
            return Result<ProjectId>.Success(project.Id);

        // Project is private, we need to verify the user's access
        var authorized = await verificationService.VerifyProjectPermissionsAsync(
            project.Id,
            requestedBy,
            PermissionsLevel.Staff
        );

        if (authorized)
            return Result<ProjectId>.Success(project.Id);

        // Unauthorized, pretend we do not see it
        return Result<ProjectId>.Fail(ResultStatus.ProjectNotFound);
    }
}
