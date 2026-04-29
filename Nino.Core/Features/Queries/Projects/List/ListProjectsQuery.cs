// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.List;

public sealed record ListProjectsQuery(GroupId GroupId, UserId RequestedBy) : IQuery;
