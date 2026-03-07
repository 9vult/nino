// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;

namespace Nino.Domain.Entities;

public sealed class Role
{
    [MaxLength(16)]
    public required string Abbreviation { get; set; }

    [MaxLength(32)]
    public required string Name { get; set; }

    public required decimal Weight { get; set; }
}
