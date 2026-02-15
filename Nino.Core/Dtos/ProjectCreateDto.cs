// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Dtos;

public sealed class ProjectCreateDto
{
    public required string Nickname;
    public required int AniListId;
    public required bool IsPrivate;
    public required Guid UpdateChannelId;
    public required Guid ReleaseChannelId;

    public string? Title;
    public ProjectType? Type;
    public int? Length;
    public string? PosterUri;
    public decimal FirstEpisode;
}
