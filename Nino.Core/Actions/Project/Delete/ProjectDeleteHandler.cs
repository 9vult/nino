// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Events;

namespace Nino.Core.Actions.Project.Delete;

public sealed class ProjectDeleteHandler(
    DataContext db,
    IEventBus bus,
    ILogger<ProjectDeleteHandler> logger
)
{
    public async Task<bool> Handle(ProjectDeleteAction action)
    {
        // check here
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == action.ProjectId);

        if (project is null)
            return false;

        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        // publish event

        return true;
    }
}
