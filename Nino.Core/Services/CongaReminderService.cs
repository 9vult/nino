// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Services;

public sealed class CongaReminderService(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<CongaReminderService> logger
) : BackgroundService
{
    private static readonly TimeSpan FifteenMinutes = TimeSpan.FromMinutes(15);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Conga Reminder Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(FifteenMinutes, stoppingToken);
            await CheckForTardyTasksAsync();
        }
    }

    private async Task CheckForTardyTasksAsync()
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<NinoDbContext>();

            var episodes = await db
                .Episodes.Include(e => e.Project)
                .Where(e =>
                    !e.Project.IsArchived
                    && e.Project.CongaRemindersEnabled
                    && e.Project.CongaParticipants.Nodes.Count > 0
                    && !e.IsDone
                )
                .ToListAsync();

            List<Task> awaitable = [];
            foreach (var episode in episodes)
            {
                var tasks = GetTardyTasks(episode.Project, episode);
                foreach (var task in tasks)
                    task.LastRemindedAt = DateTimeOffset.UtcNow;

                awaitable.Add(
                    eventBus.PublishAsync(
                        new CongaNotificationEvent(
                            ProjectId: episode.Project.Id,
                            EpisodeId: episode.Id,
                            TaskIds: tasks.Select(t => t.Id).ToList(),
                            IsReminder: true
                        )
                    )
                );
            }

            awaitable.Add(db.SaveChangesAsync());
            await Task.WhenAll(awaitable);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Conga notification service error: {Message}", ex.Message);
        }
    }

    private static List<Domain.Entities.Task> GetTardyTasks(Project project, Episode episode)
    {
        var taskLookup = episode.Tasks.ToDictionary(t => t.Abbreviation, t => t);
        var graph = project.CongaParticipants;
        var nextTasks = new List<Domain.Entities.Task>();

        foreach (var nextTask in taskLookup.Keys)
        {
            if (!graph.TryGetNode(nextTask, out var taskNode))
                continue;
            var prerequisites = taskNode.Prerequisites;
            if (prerequisites.Count == 0)
                continue;

            // Check if all prereqs are complete and that the task exists
            if (
                !prerequisites.All(p =>
                    !taskLookup.TryGetValue(p.Name, out var pTask) || pTask.IsDone
                )
            )
                continue;
            if (!taskLookup.TryGetValue(nextTask, out var task) || task.IsDone)
                continue;

            // Check if the task is tardy (can't be tardy if you've never been reminded!)
            if (
                !taskLookup.TryGetValue(nextTask, out var candidate)
                || candidate.LastRemindedAt is null
            )
                continue;

            if (
                candidate.LastRemindedAt?.AddMinutes(-2)
                < DateTimeOffset.UtcNow - project.CongaReminderPeriod
            )
                nextTasks.Add(candidate);
        }
        return nextTasks;
    }
}
