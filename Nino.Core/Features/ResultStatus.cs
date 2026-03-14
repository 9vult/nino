// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features;

/// <summary>
/// <see cref="Result{TValue}"/> status
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// Task completed successfully
    /// </summary>
    Success = 0,

    /// <summary>
    /// Generic error condition
    /// </summary>
    Error = 1,

    /// <summary>
    /// User is not authorized to perform an action
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// User provided invalid inputs
    /// </summary>
    BadRequest = 3,

    /// <summary>
    /// Project is archived
    /// </summary>
    Archived = 4,

    /// <summary>
    /// Action can't be completed due to a naming conflict
    /// </summary>
    Conflict = 5,

    /// <summary>
    /// Project does not exist
    /// </summary>
    ProjectNotFound = 6,

    /// <summary>
    /// Episode does not exist
    /// </summary>
    EpisodeNotFound = 7,

    /// <summary>
    /// Task does not exist
    /// </summary>
    TaskNotFound = 8,

    /// <summary>
    /// Staff does not exist
    /// </summary>
    StaffNotFound = 9,

    /// <summary>
    /// Generic not found condition
    /// </summary>
    NotFound = 10,

    /// <summary>
    /// Error occured with AniList
    /// </summary>
    AniListError = 11,
}
