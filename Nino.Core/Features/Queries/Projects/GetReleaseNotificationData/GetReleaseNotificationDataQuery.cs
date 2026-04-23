// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.GetReleaseNotificationData;

public sealed record GetReleaseNotificationDataQuery(
    ProjectId ProjectId,
    RoleId? PrimaryRoleId,
    RoleId? SecondaryRoleId,
    RoleId? TertiaryRoleId
) : IQuery;
