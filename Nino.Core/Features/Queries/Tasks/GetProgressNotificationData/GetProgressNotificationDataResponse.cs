// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Queries.Tasks.GetProgressNotificationData;

public sealed record GetProgressNotificationDataResponse(
    GetGenericProjectDataResponse ProjectData,
    Abbreviation Abbreviation,
    string TaskName,
    Number EpisodeNumber,
    ProgressPublishType PublishType,
    MappedIdDto<ChannelId> NotificationChannel,
    Locale Locale
);
