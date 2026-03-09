// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Project.Create;

public sealed record CreateProjectCommand(
    GroupId GroupId,
    UserId OwnerId,
    bool OverrideVerification
)
{
    public required string Nickname { get; set; }
    public required AniListId AniListId { get; init; }
    public required bool IsPrivate { get; init; }
    public required ChannelId ProjectChannelId { get; init; }
    public required ChannelId UpdateChannelId { get; init; }
    public required ChannelId ReleaseChannelId { get; init; }

    public string? Title { get; set; }
    public ProjectType? Type { get; set; }
    public int? Length { get; set; }
    public string? PosterUrl { get; set; }

    public decimal FirstEpisode { get; set; } = 1;
}
