// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Dtos;

public sealed class StaffCreateDto
{
    public required ulong UserId { get; set; }
    public required Role Role { get; set; }
    public required bool IsPseudo { get; set; } = false;
}
