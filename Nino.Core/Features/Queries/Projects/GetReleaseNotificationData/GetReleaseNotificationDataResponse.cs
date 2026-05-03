// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Queries.Projects.GetReleaseNotificationData;

public sealed record GetReleaseNotificationDataResponse(
    string ProjectTitle,
    string ReleasePrefix,
    MappedIdDto<ChannelId> NotificationChannel,
    MappedIdDto<RoleId>? PrimaryRole,
    MappedIdDto<RoleId>? SecondaryRole,
    MappedIdDto<RoleId>? TertiaryRole,
    Locale Locale
);
