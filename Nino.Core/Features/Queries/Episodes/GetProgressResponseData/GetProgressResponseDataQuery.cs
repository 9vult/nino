// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.GetProgressResponseData;

public sealed record GetProgressResponseDataQuery(EpisodeId EpisodeId) : IQuery;
