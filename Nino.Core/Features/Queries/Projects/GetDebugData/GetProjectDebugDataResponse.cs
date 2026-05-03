// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.GetDebugData;

public sealed record GetProjectDebugDataResponse(
    ProjectId ProjectId,
    GroupId GroupId,
    UserId OwnerId,
    ChannelId ProjectChannelId,
    ChannelId UpdateChannelId,
    ChannelId ReleaseChannelId,
    Alias Nickname,
    string Title,
    AniListId AniListId,
    int AniListOffset,
    bool IsPrivate,
    bool IsArchived,
    int EpisodeCount,
    int TemplateStaffCount,
    int TaskCount,
    int CongaCount,
    int ObserverCount
);
