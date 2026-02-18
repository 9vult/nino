// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Actions.Project.Export;

/// <summary>
/// Request to export a project
/// </summary>
/// <param name="ProjectId">ID of the project</param>
/// <param name="RequestedBy">ID of the user requesting the export</param>
public sealed record ProjectExportAction(Guid ProjectId, Guid RequestedBy);
