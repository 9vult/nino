// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.GetDebugData;

public sealed record GetDebugDataResponse(
    ProjectId ProjectId,
    GroupId GroupId,
    UserId OwnerId,
    ChannelId ProjectChannelId,
    Alias Nickname,
    string Title,
    AniListId AniListId,
    bool IsPrivate,
    bool IsArchived,
    int EpisodeCount,
    int TemplateStaffCount,
    int TaskCount
);
