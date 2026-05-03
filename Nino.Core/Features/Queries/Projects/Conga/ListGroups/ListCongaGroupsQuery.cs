// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Conga.ListGroups;

public sealed record ListCongaGroupsQuery(ProjectId ProjectId) : IQuery;
