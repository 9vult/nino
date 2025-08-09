namespace Nino.Utilities.AzureDtos;

public record ObserverDto
{
    public required Guid Id { get; set; }
    public required string GuildId { get; set; }
    public required string OriginGuildId { get; set; }
    public required string OwnerId { get; set; }
    public required Guid ProjectId { get; set; }
    public required bool Blame { get; set; }
    public string? RoleId { get; set; }
    public string? ProgressWebhook { get; set; }
    public string? ReleasesWebhook { get; set; }
}