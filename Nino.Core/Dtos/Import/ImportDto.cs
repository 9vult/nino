// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Import;

public sealed class ImportDto
{
    public required ProjectImportDto Project { get; set; }
    public EpisodeImportDto[]? Episodes { get; set; } = null;
}
