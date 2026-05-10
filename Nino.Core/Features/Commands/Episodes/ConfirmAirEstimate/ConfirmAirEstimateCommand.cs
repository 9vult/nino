// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Episodes.ConfirmAirEstimate;

public sealed record ConfirmAirEstimateCommand(EpisodeId EpisodeId, UserId RequestedBy) : ICommand;
