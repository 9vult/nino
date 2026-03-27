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
    Success,

    /// <summary>
    /// Generic error condition
    /// </summary>
    Error,

    /// <summary>
    /// User is not authorized to perform an action
    /// </summary>
    Unauthorized,

    /// <summary>
    /// User provided invalid inputs
    /// </summary>
    BadRequest,

    /// <summary>
    /// Project is archived
    /// </summary>
    Archived,

    /// <summary>
    /// Action can't be completed due to a project naming conflict
    /// </summary>
    ProjectConflict,

    /// <summary>
    /// Action can't be completed due to a episode naming conflict
    /// </summary>
    EpisodeConflict,

    /// <summary>
    /// Action can't be completed due to a task naming conflict
    /// </summary>
    TaskConflict,

    /// <summary>
    /// Action can't be completed due to a staff naming conflict
    /// </summary>
    StaffConflict,

    /// <summary>
    /// Action can't be completed due to a conga participant naming conflict
    /// </summary>
    CongaConflict,

    /// <summary>
    /// Project does not exist
    /// </summary>
    ProjectNotFound,

    /// <summary>
    /// Episode does not exist
    /// </summary>
    EpisodeNotFound,

    /// <summary>
    /// Task does not exist
    /// </summary>
    TaskNotFound,

    /// <summary>
    /// Template Staff does not exist
    /// </summary>
    TemplateStaffNotFound,

    /// <summary>
    /// Conga participant does not exist
    /// </summary>
    CongaNotFound,

    /// <summary>
    /// Group does not exist
    /// </summary>
    GroupNotFound,

    /// <summary>
    /// Generic not found condition
    /// </summary>
    NotFound,

    /// <summary>
    /// AniList API error
    /// </summary>
    AniListApiError,

    /// <summary>
    /// AniList 404
    /// </summary>
    AniListNotFound,

    /// <summary>
    /// Some other AniList error
    /// </summary>
    AniListError,

    /// <summary>
    /// Project does not exist
    /// </summary>
    ProjectResolutionFailed,

    /// <summary>
    /// Episode does not exist
    /// </summary>
    EpisodeResolutionFailed,

    /// <summary>
    /// Task does not exist
    /// </summary>
    TaskResolutionFailed,

    /// <summary>
    /// Template Staff does not exist
    /// </summary>
    TemplateStaffResolutionFailed,

    /// <summary>
    /// Project doesn't have a project channel set
    /// </summary>
    MissingProjectChannel,
}
