// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.Roster;

public sealed record EpisodeRosterStatus(
    Abbreviation Abbreviation,
    MappedIdDto<UserId> Assignee,
    decimal Weight,
    bool IsPseudo
);
