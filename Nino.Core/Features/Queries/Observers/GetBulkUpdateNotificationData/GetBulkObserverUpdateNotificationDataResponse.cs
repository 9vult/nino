// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Queries.Observers.GetBulkUpdateNotificationData;

public sealed record GetBulkObserverUpdateNotificationDataResponse(
    GetGenericProjectDataResponse ProjectData,
    Abbreviation Abbreviation,
    string TaskName,
    Number FirstEpisodeNumber,
    Number LastEpisodeNumber,
    MappedIdDto<ChannelId> NotificationChannel,
    string OriginGroupName,
    bool IncludeOriginGroupName,
    Locale Locale
);
