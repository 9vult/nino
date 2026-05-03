// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Queries.Tasks.GetBulkProgressNotificationData;

public sealed record GetBulkProgressNotificationDataResponse(
    GetGenericProjectDataResponse ProjectData,
    Abbreviation Abbreviation,
    string TaskName,
    Number FirstEpisodeNumber,
    Number LastEpisodeNumber,
    MappedIdDto<ChannelId> NotificationChannel,
    Locale Locale
);
