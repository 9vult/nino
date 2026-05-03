// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.Dtos;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Queries.Tasks.GetCongaNotificationData;

public sealed record GetCongaNotificationDataResponse(
    GetGenericProjectDataResponse ProjectData,
    Alias ProjectNickname,
    Number EpisodeNumber,
    List<TaskAssigneeDto> Staff,
    MappedIdDto<ChannelId> NotificationChannel,
    CongaPrefixType PrefixType,
    bool IsSingleEpisodeMovie,
    Locale Locale
);
