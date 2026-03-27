// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class Title
{
    [JsonPropertyName("romaji")]
    [MaxLength(128)]
    public string? Romaji { get; set; }
}
