// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class Group
{
    [Key]
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }
    public required ulong DiscordId { get; set; }

    public User Owner { get; set; } = null!;
    public Configuration Configuration { get; set; } = null!;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}
