// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.GetAirNotificationData;

public sealed record GetAirNotificationDataQuery(EpisodeId EpisodeId) : IQuery;
