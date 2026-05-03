// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class CoverImage
{
    [JsonPropertyName("extraLarge")]
    [MaxLength(256)]
    public string? ExtraLarge { get; set; }
}
