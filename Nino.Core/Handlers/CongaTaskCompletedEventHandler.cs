// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Events;

namespace Nino.Core.Handlers;

public sealed class CongaTaskCompletedEventHandler(
    DataContext db,
    IEventBus eventBus,
    ILogger<CongaTaskCompletedEventHandler> logger
) : IEventHandler<TaskCompletedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TaskCompletedEvent @event)
    {
        var (projectId, episodeId, abbreviation, _, _) = @event;

        var episode = await db.Episodes.SingleOrDefaultAsync(e => e.Id == episodeId);
        var conga = await db
            .Projects.Where(p => p.Id == projectId)
            .Select(p => p.CongaParticipants)
            .SingleOrDefaultAsync();

        if (episode is null || conga is null || conga.Nodes.Count == 0)
            return;

        var completedNode = conga.Get(abbreviation);
        if (completedNode is null)
            return;

        var activatedNodes = completedNode.GetActivatedNodes(episode);
        if (activatedNodes.Count == 0)
            return;

        logger.LogInformation(
            "Publishing notifications to {Count} Conga Participants following episode {Episode}'s {Abbreviation} task",
            activatedNodes.Count,
            episode,
            abbreviation
        );

        foreach (var node in activatedNodes)
        {
            await eventBus.PublishAsync(
                new CongaNotificationEvent(
                    projectId,
                    episodeId,
                    node.Abbreviation,
                    IsReminder: false
                )
            );
        }
    }
}
