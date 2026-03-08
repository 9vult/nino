// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Services;

/// <summary>
/// Provides methods for checking and verify <see cref="User"/> permissions
/// </summary>
public interface IUserVerificationService
{
    /// <summary>
    /// Check if the user is able to modify a task's status
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="episodeId">Episode ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="abbreviation">Task abbreviation</param>
    /// <returns><see langword="true"/> if the user has permissions to modify the given task's status</returns>
    Task<bool> VerifyTaskPermissionsAsync(
        ProjectId projectId,
        EpisodeId episodeId,
        UserId userId,
        string abbreviation
    );

    /// <summary>
    /// Check if the user meets the minimum permissions for an action
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="minimumPermissions">Level of permissions required</param>
    /// <returns><see langword="true"/> if the user has the required <paramref name="minimumPermissions"/></returns>
    Task<bool> VerifyProjectPermissionsAsync(
        ProjectId projectId,
        UserId userId,
        PermissionsLevel minimumPermissions
    );

    /// <summary>
    /// Check if the user meets the minimum permissions for an action
    /// </summary>
    /// <param name="groupId">Group ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="minimumPermissions">Level of permissions required</param>
    /// <returns><see langword="true"/> if the user has the required <paramref name="minimumPermissions"/></returns>
    Task<bool> VerifyGroupPermissionsAsync(
        GroupId groupId,
        UserId userId,
        PermissionsLevel minimumPermissions
    );

    /// <summary>
    /// Get the effective permissions level for a user
    /// </summary>
    /// <param name="project">Project</param>
    /// <param name="userId">User ID</param>
    /// <returns>Effective permissions level</returns>
    Task<PermissionsLevel> GetProjectPermissionsAsync(Project project, UserId userId);

    /// <summary>
    /// Get the effective permissions level for a user
    /// </summary>
    /// <param name="group">Group</param>
    /// <param name="userId">User ID</param>
    /// <returns>Effective permissions level</returns>
    PermissionsLevel GetGroupPermissions(Group group, UserId userId);
}
