// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.GetGenericObserverData;

public sealed record GetGenericObserverDataResponse(
    GetGenericProjectDataResponse ProjectData,
    MappedIdDto<UserId> Owner
);
