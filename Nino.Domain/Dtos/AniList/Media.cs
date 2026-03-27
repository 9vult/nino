// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class Media
{
    [JsonPropertyName("startDate")]
    public FuzzyDate? StartDate { get; set; }

    [JsonPropertyName("airingSchedule")]
    public AiringSchedule? AiringSchedule { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    [JsonPropertyName("format")]
    [MaxLength(64)]
    public string? Format { get; set; }

    [JsonPropertyName("title")]
    public Title? Title { get; set; }

    [JsonPropertyName("coverImage")]
    public CoverImage? CoverImage { get; set; }
}
