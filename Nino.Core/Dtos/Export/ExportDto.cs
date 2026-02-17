// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Export;

public sealed class ExportDto
{
    public required ProjectExportDto Project { get; init; }
    public required EpisodeExportDto[] Episodes { get; init; }
}
