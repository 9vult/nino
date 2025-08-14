using System.ComponentModel.DataAnnotations;

namespace Nino.Records;

public class Observer
{
    [Key]
    public Guid Id { get; set; }
    public required ulong GuildId { get; set; }
    public required ulong OriginGuildId { get; set; }
    public required ulong OwnerId { get; set; }
    public required Guid ProjectId { get; set; }
    public required bool Blame { get; set; }
    public ulong? RoleId { get; set; }
    public string? ProgressWebhook { get; set; }
    public string? ReleasesWebhook { get; set; }

    public Project Project { get; set; } = null!;
}
