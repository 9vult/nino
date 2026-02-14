// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;

namespace Nino.Core.Entities;

/// <summary>
/// Description of a <see cref="Staff"/> role
/// </summary>
[Owned]
public class Role
{
    [MaxLength(16)]
    public required string Abbreviation { get; set; }

    [MaxLength(32)]
    public required string Name { get; set; }
    public required decimal Weight { get; set; }
}
