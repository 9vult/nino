// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Group : MappedId<GroupId>
{
    public UserId? OwnerId { get; set; }
    public User? Owner { get; set; }

    [MaxLength(Length.GroupName)]
    public string Name { get; set; } = string.Empty;

    public required ConfigurationId ConfigurationId { get; set; }
    public Configuration Configuration { get; set; } = null!;

    public ICollection<Project> Projects { get; set; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        return $"C[{Id} ({Name},D{DiscordId})]";
    }
}
