// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Actions.Project.Delete;

public sealed class ProjectDeleteHandler(DataContext db, ILogger<ProjectDeleteHandler> logger)
{
    public async Task<ProjectDeleteResult> HandleAsync(ProjectDeleteAction action)
    {
        // check here
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == action.ProjectId);

        if (project is null)
            return new ProjectDeleteResult(ActionStatus.NotFound, null);

        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        return new ProjectDeleteResult(ActionStatus.Success, "");
    }
}
