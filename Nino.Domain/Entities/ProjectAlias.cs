// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class ProjectAlias
{
    public AliasId Id { get; set; } = AliasId.FromNewGuid();

    [MaxLength(Length.Alias)]
    public required Alias Value { get; set; }
}
