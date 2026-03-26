// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Domain.Entities;

public sealed class Configuration
{
    public ConfigurationId Id { get; set; } = ConfigurationId.FromNewGuid();

    public GroupId GroupId { get; set; }

    public required ProgressResponseType ProgressResponseType { get; set; } =
        ProgressResponseType.Succinct;

    public required ProgressPublishType ProgressPublishType { get; set; } =
        ProgressPublishType.Normal;

    public required CongaPrefixType CongaPrefixType { get; set; } = CongaPrefixType.None;

    [MaxLength(Length.ReleasePrefix)]
    public required string ReleasePrefix { get; set; } = string.Empty;

    public Locale Locale { get; set; } = Locale.EnglishUS;

    public bool PublishPrivateProgress { get; set; } = true;

    public ICollection<Administrator> Administrators { get; set; } = [];
}
