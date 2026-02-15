// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Dtos;

public sealed class ExportDto
{
    public required Project Project { get; init; }
    public required Episode[] Episodes { get; init; }
}
