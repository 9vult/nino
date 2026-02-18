// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Dtos.Export;

public sealed class ExportDto
{
    public required ProjectExportDto Project { get; init; }
    public required EpisodeExportDto[] Episodes { get; init; }

    internal static ExportDto Create(Project project)
    {
        return new ExportDto
        {
            Project = ProjectExportDto.FromProject(project),
            Episodes = project.Episodes.Select(EpisodeExportDto.FromEpisode).ToArray(),
        };
    }
}
