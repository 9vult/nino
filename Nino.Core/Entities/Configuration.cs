// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;
using Nino.Localization;

namespace Nino.Core.Entities;

/// <summary>
/// Server configuration options
/// </summary>
public class Configuration
{
    [Key]
    public Guid Id { get; set; }
    public required ulong GuildId { get; set; }
    public required ProgressResponseType ProgressResponseType { get; set; }
    public required ProgressPublishType ProgressPublishType { get; set; }
    public required CongaPrefixType CongaPrefix { get; set; }

    [MaxLength(128)]
    public required string ReleasePrefix { get; set; }
    public Locale Locale { get; set; }
    public bool PublishPrivateProgress { get; set; }

    public ICollection<Administrator> Administrators { get; set; } = new List<Administrator>();

    public static Configuration CreateDefault(ulong guildId)
    {
        return new Configuration
        {
            GuildId = guildId,
            ProgressResponseType = ProgressResponseType.Succinct,
            ProgressPublishType = ProgressPublishType.Normal,
            CongaPrefix = CongaPrefixType.None,
            ReleasePrefix = string.Empty,
            Locale = Locale.EnglishUS,
            PublishPrivateProgress = true,
        };
    }
}
