// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Core.Features.Queries.Episodes.GetProgressResponseData;

public sealed record GetProgressResponseDataResponse(
    ProgressResponseType ProgressResponseType,
    IReadOnlyList<GetProgressResponseDataStatus> Statuses
);
