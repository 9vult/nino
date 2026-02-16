// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations.Schema;
using Nino.Core.Actions;
using Nino.Core.Dtos;
using Nino.Core.Enums;

namespace Nino.Core.Entities;

public class AniListResponse
{
    [Key]
    public required int AniListId { get; set; }
    public required AniListRoot? Data { get; set; }
    public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;

    [NotMapped]
    public ResultStatus Status { get; set; }

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
