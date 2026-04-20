// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Groups.GetGenericData;

public sealed record class GetGenericGroupDataQuery(GroupId GroupId) : IQuery;
