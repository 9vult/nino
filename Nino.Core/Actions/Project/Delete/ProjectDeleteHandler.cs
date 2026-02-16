// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;

namespace Nino.Core.Actions.Project.Delete;

public sealed class ProjectDeleteHandler(DataContext db, ILogger<ProjectDeleteHandler> logger)
{
    public async Task<Result<string>> HandleAsync(ProjectDeleteAction action)
    {
        // check here
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == action.ProjectId);

        if (project is null)
            return new Result<string>(ResultStatus.NotFound, null);

        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        return new Result<string>(ResultStatus.Success, "");
    }
}
