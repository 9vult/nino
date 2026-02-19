// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Nino.Core.Dtos.Export;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Features.Project.Export;

public class ExportProjectHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<ExportProjectHandler> logger
)
{
    public async Task<Result<string>> HandleAsync(ExportProjectCommand input)
    {
        var (projectId, userId) = input;
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                userId,
                PermissionsLevel.Administrator
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
        return new Result<string>(ResultStatus.Success, JsonSerializer.Serialize(export));
    }
}
