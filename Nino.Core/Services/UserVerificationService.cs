// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Features;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Services;

public class UserVerificationService(
    ReadOnlyNinoDbContext db,
    ILogger<UserVerificationService> logger
) : IUserVerificationService
{
    /// <inheritdoc />
    public async Task<Result> VerifyTaskPermissionsAsync(
        ProjectId projectId,
        EpisodeId episodeId,
        TaskId taskId,
        UserId userId
    )
    {
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} was not found", projectId);
            return Fail(ResultStatus.ProjectNotFound);
        }
        var episode = await db.Episodes.SingleOrDefaultAsync(e => e.Id == episodeId);
        if (episode is null)
        {
            logger.LogWarning("Episode {EpisodeId} was not found", episodeId);
            return Fail(ResultStatus.EpisodeNotFound);
        }
        var task = episode.Tasks.SingleOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            logger.LogWarning("Task {TasKId} was not found", taskId);
            return Fail(ResultStatus.TaskNotFound);
        }

        if (task.AssigneeId == userId)
            return Success();

        var projectPermissions = await GetProjectPermissionsAsync(project, userId);
        if (projectPermissions >= PermissionsLevel.Administrator)
            return Success();

        return Fail(ResultStatus.Unauthorized);
    }

    /// <inheritdoc />
    public async Task<Result> VerifyProjectPermissionsAsync(
        ProjectId projectId,
        UserId userId,
        PermissionsLevel minimumPermissions
    )
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} was not found", projectId);
            return Fail(ResultStatus.ProjectNotFound);
        }

        var effectivePermissions = await GetProjectPermissionsAsync(project, userId);
        return effectivePermissions >= minimumPermissions
            ? Success()
            : Fail(ResultStatus.Unauthorized);
    }

    /// <inheritdoc />
    public async Task<Result> VerifyGroupPermissionsAsync(
        GroupId groupId,
        UserId userId,
        PermissionsLevel minimumPermissions
    )
    {
        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group is null)
        {
            logger.LogWarning("Group {GroupId} was not found", groupId);
            return Fail(ResultStatus.GroupNotFound);
        }

        var effectivePermissions = GetGroupPermissions(group, userId);
        return effectivePermissions >= minimumPermissions
            ? Success()
            : Fail(ResultStatus.Unauthorized);
    }

    /// <inheritdoc />
    public async Task<PermissionsLevel> GetProjectPermissionsAsync(Project project, UserId userId)
    {
        if (project.OwnerId == userId)
            return PermissionsLevel.Owner;

        if (project.Administrators.Any(a => a.UserId == userId))
            return PermissionsLevel.Administrator;

        var groupAdmins = await db
            .Groups.Where(g => g.Id == project.GroupId)
            .Select(g => g.Configuration.Administrators)
            .FirstOrDefaultAsync();
        if ((groupAdmins ?? []).Any(a => a.UserId == userId))
            return PermissionsLevel.Administrator;

        var isStaff = await db
            .Episodes.Where(e => e.ProjectId == project.Id)
            .AnyAsync(e => e.Tasks.Any(s => s.AssigneeId == userId));

        if (isStaff)
            return PermissionsLevel.Staff;

        return project.IsPrivate ? PermissionsLevel.None : PermissionsLevel.User;
    }

    /// <inheritdoc />
    public PermissionsLevel GetGroupPermissions(Group group, UserId userId)
    {
        return group.Configuration.Administrators.Any(a => a.UserId == userId)
            ? PermissionsLevel.Administrator
            : PermissionsLevel.User;
    }
}
