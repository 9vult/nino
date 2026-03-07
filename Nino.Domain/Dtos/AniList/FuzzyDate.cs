// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class FuzzyDate
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("month")]
    public int Month { get; set; }

    [JsonPropertyName("day")]
    public int Day { get; set; }
}
