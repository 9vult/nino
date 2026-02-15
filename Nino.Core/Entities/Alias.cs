// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// Additional aliases a <see cref="Project"/> can be referred to by
/// </summary>
public class Alias
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(32)]
    public required string Value { get; set; }
}
