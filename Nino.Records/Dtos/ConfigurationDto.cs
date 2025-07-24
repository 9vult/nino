using Localizer;
using Nino.Records.Enums;

namespace Nino.Records.Dtos;

public record ConfigurationDto
{
    public required string Id { get; set; }
    public required string GuildId { get; set; }
    public required UpdatesDisplayType UpdateDisplay { get; set; }
    public required ProgressDisplayType ProgressDisplay { get; set; }
    public required CongaPrefixType CongaPrefix { get; set; }
    public required List<string> AdministratorIds { get; set; }
    public string? ReleasePrefix { get; set; }
    public Locale? Locale { get; set; }
}