// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.Blame;

public sealed record BlameStatus(
    Abbreviation Abbreviation,
    string Name,
    bool IsDone,
    decimal Weight,
    bool IsPseudo
);
