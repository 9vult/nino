// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.GetWorkingTaskEpisode;

public sealed record GetWorkingTaskEpisodeQuery(ProjectId ProjectId, Abbreviation Abbreviation)
    : IQuery;
