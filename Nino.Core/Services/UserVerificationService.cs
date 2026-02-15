// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;

namespace Nino.Core.Services;

public class UserVerificationService(DataContext db, ILogger<UserVerificationService> logger)
    : IUserVerificationService
{
    /// <inheritdoc />
    public async Task<bool> VerifyProjectPermissionsAsync(
        Guid projectId,
        Guid userId,
        PermissionsLevel minimumPermissions
    )
    {
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
        if (project is null)
        {
            logger.LogWarning("Project {ProjectId} was not found", projectId);
            return false;
        }

        var effectivePermissions = await GetProjectPermissionsAsync(project, userId);
        return effectivePermissions >= minimumPermissions;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyGroupPermissionsAsync(
        Guid groupId,
        Guid userId,
        PermissionsLevel minimumPermissions
    )
    {
        var group = await db.Groups.SingleOrDefaultAsync(g => g.Id == groupId);
        if (group is null)
        {
            logger.LogWarning("Group {GroupId} was not found", groupId);
            return false;
        }

        var effectivePermissions = GetGroupPermissions(group, userId);
        return effectivePermissions >= minimumPermissions;
    }

    /// <inheritdoc />
    public async Task<PermissionsLevel> GetProjectPermissionsAsync(Project project, Guid userId)
    {
        if (project.OwnerId == userId)
            return PermissionsLevel.Owner;

        if (project.Administrators.Any(a => a.UserId == userId))
            return PermissionsLevel.Administrator;

        var groupAdmins = await db
            .Groups.Where(g => g.Id == project.GroupId)
            .Select(g => g.Configuration.Administrators)
            .SingleOrDefaultAsync();
        if ((groupAdmins ?? []).Any(a => a.UserId == userId))
            return PermissionsLevel.Administrator;

        if (project.KeyStaff.Any(s => s.UserId == userId))
            return PermissionsLevel.Staff;

        var isEpisodeStaff = await db
            .Episodes.Where(e => e.ProjectId == project.Id)
            .AnyAsync(e =>
                e.AdditionalStaff.Any(s => s.UserId == userId)
                || e.PinchHitters.Any(s => s.UserId == userId)
            );

        if (isEpisodeStaff)
            return PermissionsLevel.Staff;

        return project.IsPrivate ? PermissionsLevel.None : PermissionsLevel.User;
    }

    /// <inheritdoc />
    public PermissionsLevel GetGroupPermissions(Group group, Guid userId)
    {
        return group.Configuration.Administrators.Any(a => a.UserId == userId)
            ? PermissionsLevel.Administrator
            : PermissionsLevel.User;
    }
}
