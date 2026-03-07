// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Configuration
{
    public ConfigurationId Id { get; set; } = ConfigurationId.New();

    public GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public required ProgressResponseType ProgressResponseType { get; set; }
    public required ProgressPublishType ProgressPublishType { get; set; }
    public required CongaPrefixType CongaPrefix { get; set; }

    [MaxLength(128)]
    public required string ReleasePrefix { get; set; }

    // public Locale Locale { get; set; }
    public bool PublishPrivateProgress { get; set; }

    public ICollection<Administrator> Administrators { get; set; } = [];

    public static Configuration CreateDefault()
    {
        return new Configuration
        {
            ProgressResponseType = ProgressResponseType.Succinct,
            ProgressPublishType = ProgressPublishType.Normal,
            CongaPrefix = CongaPrefixType.None,
            ReleasePrefix = string.Empty,
            // Locale = Locale.EnglishUS,
            PublishPrivateProgress = true,
        };
    }
}
