// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.GetDebugData;

public sealed record GetProjectDebugDataQuery(ProjectId ProjectId) : IQuery;
