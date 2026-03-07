// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Alias
{
    public AliasId Id { get; set; } = AliasId.New();

    [MaxLength(32)]
    public required string Value { get; set; }
}
