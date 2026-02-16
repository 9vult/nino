// SPDX-License-Identifier: MPL-2.0

// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Text.Json.Serialization;

namespace Nino.Core.Dtos;

public class AniListRoot
{
    [JsonPropertyName("data")]
    public Data? Data { get; set; }
}

public class Data
{
    [JsonPropertyName("Media")]
    public Media? Media { get; set; }
}

public class Media
{
    [JsonPropertyName("startDate")]
    public FuzzyDate? StartDate { get; set; }

    [JsonPropertyName("airingSchedule")]
    public AiringSchedule? AiringSchedule { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("episodes")]
    public int Episodes { get; set; }

    [JsonPropertyName("format")]
    [MaxLength(64)]
    public string? Format { get; set; }

    [JsonPropertyName("title")]
    public Title? Title { get; set; }

    [JsonPropertyName("coverImage")]
    public CoverImage? CoverImage { get; set; }
}

public class FuzzyDate
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("month")]
    public int Month { get; set; }

    [JsonPropertyName("day")]
    public int Day { get; set; }
}

public class AiringSchedule
{
    [JsonPropertyName("pageInfo")]
    public PageInfo? PageInfo { get; set; }

    [JsonPropertyName("nodes")]
    public List<AiringNode>? Nodes { get; set; }
}

public class PageInfo
{
    [JsonPropertyName("lastPage")]
    public int LastPage { get; set; }
}

public class AiringNode
{
    [JsonPropertyName("episode")]
    public int Episode { get; set; }

    /// <summary>
    /// Unix timestamp (seconds)
    /// </summary>
    [JsonPropertyName("airingAt")]
    public long AiringAt { get; set; }
}

public class Title
{
    [JsonPropertyName("romaji")]
    [MaxLength(128)]
    public string? Romaji { get; set; }
}

public class CoverImage
{
    [JsonPropertyName("extraLarge")]
    [MaxLength(256)]
    public string? ExtraLarge { get; set; }
}
