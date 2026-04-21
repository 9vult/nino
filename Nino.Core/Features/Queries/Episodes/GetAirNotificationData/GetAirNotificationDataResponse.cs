// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Queries.Episodes.GetAirNotificationData;

public sealed record GetAirNotificationDataResponse(
    GetGenericProjectDataResponse ProjectData,
    Number EpisodeNumber,
    MappedIdDto<ChannelId> NotificationChannel,
    MappedIdDto<UserId>? NotificationUser,
    MappedIdDto<RoleId>? NotificationRole,
    Locale Locale
);
