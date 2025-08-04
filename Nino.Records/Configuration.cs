using Localizer;
using Nino.Records.Enums;

namespace Nino.Records;

public class Configuration
{
    public required string Id { get; set; }
    public required ulong GuildId { get; set; }
    public required UpdatesDisplayType UpdateDisplay { get; set; }
    public required ProgressDisplayType ProgressDisplay { get; set; }
    public required CongaPrefixType CongaPrefix { get; set; }
    public required ulong[] AdministratorIds { get; set; }
    public string? ReleasePrefix { get; set; }
    public Locale? Locale { get; set; }

    /// <summary>
    /// Create a default configuration for a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns>Default configuration options</returns>
    public static Configuration CreateDefault(ulong guildId) =>
        new()
        {
            Id = $"{guildId}-conf",
            GuildId = guildId,
            UpdateDisplay = UpdatesDisplayType.Normal,
            ProgressDisplay = ProgressDisplayType.Succinct,
            CongaPrefix = CongaPrefixType.None,
            AdministratorIds = [],
            ReleasePrefix = null
        };
}