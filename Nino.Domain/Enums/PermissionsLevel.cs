// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

/// <summary>
/// Levels of permissions
/// </summary>
/// <remarks>
/// Can represent either the permissions held by a user
/// or the permissions required to perform an action.
/// </remarks>
public enum PermissionsLevel
{
    /// <summary>
    /// No permissions (generic user, private project)
    /// </summary>
    None = 0,

    /// <summary>
    /// Baseline (generic user, public project)
    /// </summary>
    User = 1,

    /// <summary>
    /// Project Staff
    /// </summary>
    Staff = 2,

    /// <summary>
    /// Administrator
    /// </summary>
    Administrator = 3,

    /// <summary>
    /// Project owner
    /// </summary>
    Owner = 4,
}
