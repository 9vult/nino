// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class AiringSchedule
{
    [JsonPropertyName("pageInfo")]
    public PageInfo? PageInfo { get; set; }

    [JsonPropertyName("nodes")]
    public List<AiringNode>? Nodes { get; set; }
}
