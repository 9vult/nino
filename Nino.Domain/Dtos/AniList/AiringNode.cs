// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

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
