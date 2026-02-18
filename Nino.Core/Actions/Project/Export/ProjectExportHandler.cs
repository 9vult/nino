// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Nino.Core.Dtos.Export;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Actions.Project.Export;

public class ProjectExportHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<ProjectExportHandler> logger
)
{
    public async Task<Result<string>> HandleAsync(ProjectExportAction action)
    {
        var (projectId, userId) = action;
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                userId,
                PermissionsLevel.Owner
            )
        )
            return new Result<string>(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == action.ProjectId);

        if (project is null)
            return new Result<string>(ResultStatus.NotFound);

        logger.LogInformation("Generating JSON export of project {Project}", project);

        var export = ExportDto.Create(project);
        return new Result<string>(ResultStatus.Success, JsonSerializer.Serialize(export));
    }
}
