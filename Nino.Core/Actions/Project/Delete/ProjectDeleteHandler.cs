// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;
using Nino.Core.Events;

namespace Nino.Core.Actions.Project.Delete;

public sealed class ProjectDeleteHandler(
    DataContext db,
    IEventBus bus,
    ILogger<ProjectDeleteHandler> logger
)
{
    public async Task<ActionResult> Handle(ProjectDeleteAction action)
    {
        // check here
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == action.ProjectId);

        if (project is null)
            return new ActionResult(ActionStatus.NotFound);

        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        // publish event

        return ActionResult.Success;
    }
}
