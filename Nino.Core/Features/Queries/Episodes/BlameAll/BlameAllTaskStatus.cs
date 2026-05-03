// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Episodes.BlameAll;

public sealed record BlameAllTaskStatus(
    Abbreviation Abbreviation,
    string Name,
    bool IsDone,
    decimal Weight,
    bool IsPseudo
);
