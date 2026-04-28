// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Domain.Enums;

namespace Nino.Core.EventHandlers;

public class TaskProgressCongaEventHandler(
    NinoDbContext db,
    IEventBus eventBus,
    ILogger<TaskProgressCongaEventHandler> logger
) : IEventHandler<TaskProgressCongaEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TaskProgressCongaEvent @event)
    {
        var (projectId, episodeId, taskId, progressType) = @event;

        // We only care about forward progress
        if (progressType is ProgressType.Undone)
            return;

        var data = await db
            .Projects.Where(p => p.Id == projectId)
            .Select(p => new
            {
                Episode = p.Episodes.FirstOrDefault(e => e.Id == episodeId),
                p.CongaParticipants,
            })
            .FirstOrDefaultAsync();

        var task = data?.Episode?.Tasks.FirstOrDefault(t => t.Id == taskId);

        if (data?.Episode is null || task is null)
            return;

        if (data.CongaParticipants.Nodes.Count == 0)
            return;

        if (!data.CongaParticipants.TryGetNode(task.Abbreviation, out var completedNode))
            return;

        var activatedNodes = completedNode.GetActivatedNodes(data.Episode.Tasks.ToList());
        if (activatedNodes.Count == 0)
            return;

        var remindedTasks = activatedNodes
            .Select(n => data.Episode.Tasks.SingleOrDefault(t => t.Abbreviation == n.Name))
            .OfType<Domain.Entities.Task>() // Eliminate nulls
            .ToList();

        if (remindedTasks.Count == 0)
            return;

        foreach (var remindedTask in remindedTasks)
            remindedTask.LastRemindedAt = DateTimeOffset.UtcNow;

        var congaEvent = new CongaNotificationEvent(
            data.Episode.ProjectId,
            data.Episode.Id,
            remindedTasks.Select(t => t.Id).ToList(),
            IsReminder: false
        );
        await eventBus.PublishAsync(congaEvent);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Published notifications to {Count} Conga Participants following completion of {Episode}'s {Task}",
            remindedTasks.Count,
            data.Episode,
            task.Abbreviation
        );
    }
}
