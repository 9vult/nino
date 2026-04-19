// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.GetGenericData;

public sealed record GetGenericProjectDataQuery(ProjectId ProjectId) : IQuery;
