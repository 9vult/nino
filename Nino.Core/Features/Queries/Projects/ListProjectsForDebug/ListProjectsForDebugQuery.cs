// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.ListProjectsForDebug;

public sealed record ListProjectsForDebugQuery(GroupId GroupId, UserId RequestedBy) : IQuery;
