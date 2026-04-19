// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.ListForProject;

public sealed record ListTasksForProjectQuery(ProjectId ProjectId) : IQuery;
