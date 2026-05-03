// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class AniListRoot
{
    [JsonPropertyName("data")]
    public Data? Data { get; set; }
}
