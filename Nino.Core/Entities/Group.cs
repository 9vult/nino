// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class Group : MappedId
{
    public Guid? OwnerId { get; set; }

    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    public User? Owner { get; set; } = null!;
    public Configuration Configuration { get; set; } = null!;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        return $"G[{Id} ({Name},{DiscordId})]";
    }
}
