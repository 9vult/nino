// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.List;

public sealed record ListEpisodesQuery(ProjectId ProjectId) : IQuery;
