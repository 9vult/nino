// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Nino.Domain.Entities;

[Owned]
public sealed class Role
{
    [MaxLength(Length.Abbreviation)]
    public required string Abbreviation { get; set; }

    [MaxLength(Length.RoleName)]
    public required string Name { get; set; }

    public required decimal Weight { get; set; }
}
