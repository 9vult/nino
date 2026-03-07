// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class User : MappedId<UserId>
{
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"U[{Id} ({Name},D{DiscordId})]";
    }
}
