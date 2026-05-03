// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Groups.ListGroupsForDebug;

public sealed record ListGroupsForDebugResult(string Name, GroupId GroupId);
