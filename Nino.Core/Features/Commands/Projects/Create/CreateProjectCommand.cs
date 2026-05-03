// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Projects.Create;

public sealed record CreateProjectCommand(
    GroupId GroupId,
    UserId RequestedBy,
    bool OverrideVerification,
    Alias Nickname,
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
) : ICommand;
