// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Features.Episodes.Roster;

public sealed record EpisodeRosterCommand(Guid ProjectId, string EpisodeNumber, Guid RequestedBy);
