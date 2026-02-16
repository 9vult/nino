// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations.Schema;
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
    public ActionStatus Status { get; set; }
}
