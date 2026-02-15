// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Actions.Project.Delete;

/// <summary>
/// Request to delete a project
/// </summary>
/// <param name="ProjectId">ID of the project</param>
/// <param name="RequestedBy">ID of the user requesting deletion</param>
public sealed record ProjectDeleteAction(Guid ProjectId, Guid RequestedBy);
