using System.ComponentModel.DataAnnotations;
using Localizer;
using Nino.Records.Enums;

namespace Nino.Records;

public class Configuration
{
    [Key]
    public Guid Id { get; set; }
    public required ulong GuildId { get; set; }
    public required UpdatesDisplayType UpdateDisplay { get; set; }
    public required ProgressDisplayType ProgressDisplay { get; set; }
    public required CongaPrefixType CongaPrefix { get; set; }

    [MaxLength(128)]
    public string? ReleasePrefix { get; set; }
    public Locale? Locale { get; set; }
    public bool? PublishPrivateProgress { get; set; }

    public ICollection<Administrator> Administrators = new List<Administrator>();

    /// <summary>
    /// Create a default configuration for a guild
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <returns>Default configuration options</returns>
    public static Configuration CreateDefault(ulong guildId) =>
        new()
        {
            GuildId = guildId,
            UpdateDisplay = UpdatesDisplayType.Normal,
            ProgressDisplay = ProgressDisplayType.Succinct,
            CongaPrefix = CongaPrefixType.None,
            ReleasePrefix = null,
            PublishPrivateProgress = true,
        };
}
