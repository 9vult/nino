// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Localization;

namespace Nino.Core.Dtos;

public sealed record AirNotificationDto(
    string ProjectTitle,
    ProjectType ProjectType,
    string AniListUrl,
    string PosterUrl,
    string EpisodeNumber,
    Channel? NotificationChannel,
    User? NotificationUser,
    MentionRole? NotificationRole,
    Locale? NotificationLocale
);
