// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Project.Create;

public sealed record CreateProjectCommand(
    GroupId GroupId,
    UserId OwnerId,
    bool OverrideVerification,
    string Nickname,
    AniListId AniListId,
    bool IsPrivate,
    ChannelId ProjectChannelId,
    ChannelId UpdateChannelId,
    ChannelId ReleaseChannelId,
    string? Title,
    ProjectType? Type,
    int? Length,
    string? PosterUrl,
    decimal FirstEpisode = 1
);
