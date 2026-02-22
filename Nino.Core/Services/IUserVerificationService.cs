// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;

namespace Nino.Core.Services;

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
        Guid projectId,
        Guid episodeId,
        Guid userId,
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
        Guid projectId,
        Guid userId,
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
        Guid groupId,
        Guid userId,
        PermissionsLevel minimumPermissions
    );

    /// <summary>
    /// Get the effective permissions level for a user
    /// </summary>
    /// <param name="project">Project</param>
    /// <param name="userId">User ID</param>
    /// <returns>Effective permissions level</returns>
    Task<PermissionsLevel> GetProjectPermissionsAsync(Project project, Guid userId);

    /// <summary>
    /// Get the effective permissions level for a user
    /// </summary>
    /// <param name="group">Group</param>
    /// <param name="userId">User ID</param>
    /// <returns>Effective permissions level</returns>
    PermissionsLevel GetGroupPermissions(Group group, Guid userId);
}
