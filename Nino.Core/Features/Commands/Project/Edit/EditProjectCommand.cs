// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Project.Edit;

public sealed record EditProjectCommand(
    ProjectId ProjectId,
    UserId RequestedBy,
    string? Nickname = null,
    string? Title = null,
    string? PosterUrl = null,
    string? Motd = null,
    AniListId? AniListId = null,
    int? AniListOffset = null,
    bool? IsPrivate = null,
    ChannelId? ProjectChannelId = null,
    ChannelId? UpdateChannelId = null,
    ChannelId? ReleaseChannelId = null
);
