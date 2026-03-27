// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Nino.Domain.Dtos.AniList;

public class Data
{
    [JsonPropertyName("Media")]
    public Media? Media { get; set; }
}
