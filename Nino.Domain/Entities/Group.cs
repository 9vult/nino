// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Group : MappedId<GroupId>
{
    public UserId? OwnerId { get; set; }

    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    public User? Owner { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"C[{Id} ({Name},D{DiscordId})]";
    }
}
