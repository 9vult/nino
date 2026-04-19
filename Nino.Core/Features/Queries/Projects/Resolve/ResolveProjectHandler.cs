// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Domain.ValueObjects.ProjectId>;

namespace Nino.Core.Features.Queries.Projects.Resolve;

public sealed class ResolveProjectHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<ResolveProjectHandler> logger
) : IQueryHandler<ResolveProjectQuery, Result<ProjectId>>
{
    /// <summary>
    /// Resolve an alias to a project
    /// </summary>
    /// <param name="query">Query object</param>
    /// <returns>The <see cref="ProjectId"/> of the project, or <see cref="ResultStatus.ProjectResolutionFailed"/></returns>
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
            .FirstOrDefaultAsync();

        if (project is null)
        {
            logger.LogTrace("Unable to resolve alias {Alias} to a project", alias);
            return Fail(ResultStatus.ProjectResolutionFailed);
        }

        if (!project.IsPrivate)
            return Success(project.Id);

        // Project is private, we need to verify the user's access
        var authorized = await verificationService.VerifyProjectPermissionsAsync(
            project.Id,
            requestedBy,
            PermissionsLevel.Staff
        );

        // If unauthorized, pretend we do not see it
        return authorized.IsSuccess
            ? Success(project.Id)
            : Fail(ResultStatus.ProjectResolutionFailed);
    }
}
