// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Groups.GetGenericData;

public record GetGenericGroupDataResponse(GroupId GroupId, string GroupName);
