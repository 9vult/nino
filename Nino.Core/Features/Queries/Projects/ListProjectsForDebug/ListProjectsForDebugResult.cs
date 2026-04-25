// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.ListProjectsForDebug;

public record ListProjectsForDebugResult(Alias Alias, ProjectId ProjectId);
