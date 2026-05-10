// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Episodes.RejectAirEstimate;

public sealed record RejectAirEstimateCommand(EpisodeId EpisodeId, UserId RequestedBy) : ICommand;
