// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Nino.Core.Dtos.Export;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.Project.Delete;

public sealed class DeleteProjectHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<DeleteProjectHandler> logger
)
{
    public async Task<Result<string>> HandleAsync(DeleteProjectCommand input)
    {
        var (projectId, userId) = input;
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
            .SingleOrDefaultAsync(p => p.Id == input.ProjectId);
        if (project is null)
            return new Result<string>(ResultStatus.NotFound);

        logger.LogInformation("Generating JSON export of project {Project}", project);

        var export = ExportDto.Create(project);

        logger.LogInformation("Deleting project {Project}", project);

        await db.Projects.Where(p => p.Id == project.Id).ExecuteDeleteAsync();
        return new Result<string>(ResultStatus.Success, JsonSerializer.Serialize(export));
    }
}
