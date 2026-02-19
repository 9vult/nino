// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Project.Delete;

/// <summary>
/// Request to delete a project
/// </summary>
/// <param name="ProjectId">ID of the project</param>
/// <param name="RequestedBy">ID of the user requesting deletion</param>
public sealed record DeleteProjectCommand(Guid ProjectId, Guid RequestedBy);
