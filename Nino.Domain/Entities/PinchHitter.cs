// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class PinchHitter
{
    public PinchHitterId Id { get; set; } = PinchHitterId.New();
    public required UserId UserId { get; set; }

    public EpisodeId EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;

    [MaxLength(16)]
    public required string Abbreviation { get; set; }

    public User User { get; set; } = null!;
}
