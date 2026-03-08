// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations.Schema;
using Nino.Domain.Dtos.AniList;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public class AniListResponse
{
    public AniListId Id { get; init; }

    public required AniListRoot? Data { get; set; }
    public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;

    [NotMapped]
    public string? Title => Data?.Data?.Media?.Title?.Romaji;

    [NotMapped]
    public int? EpisodeCount => Data?.Data?.Media?.Episodes;

    [NotMapped]
    public string? PosterUrl => Data?.Data?.Media?.CoverImage?.ExtraLarge;

    [NotMapped]
    public ProjectType Type =>
        Data?.Data?.Media?.Format is not null
            ? Data?.Data?.Media?.Format switch
            {
                "TV" or "TV_SHORT" => ProjectType.TV,
                "MOVIE" => ProjectType.Movie,
                "ONA" => ProjectType.ONA,
                "OVA" or "SPECIAL" or "MUSIC" => ProjectType.OVA,
                _ => ProjectType.TV,
            }
            : ProjectType.TV;
}
