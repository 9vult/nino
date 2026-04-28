// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.GetProgressResponseData;
using Nino.Core.Features.Queries.Tasks.GetProgressNotificationData;
using Nino.Discord.Interactions;
using Nino.Discord.Services;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public sealed class TaskProgressEventHandler(
    DiscordSocketClient client,
    IBotPermissionsService botPermissionsService,
    GetProgressNotificationDataHandler getDataHandler,
    GetProgressResponseDataHandler getProgressResponseDataHandler,
    ILogger<TaskProgressEventHandler> logger
) : IEventHandler<TaskProgressEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TaskProgressEvent @event)
    {
        var (_, episodeId, taskId, progressType) = @event;

        var queryResult = await getDataHandler
            .HandleAsync(new GetProgressNotificationDataQuery(taskId))
            .ThenAsync(_ =>
                getProgressResponseDataHandler.HandleAsync(
                    new GetProgressResponseDataQuery(episodeId, false)
                )
            );
        if (!queryResult.IsSuccess)
        {
            logger.LogWarning("Failed to get progress data for task {TaskId}", taskId);
            return;
        }

        var data = queryResult.Value.Item1;
        var blame = queryResult.Value.Item2;

        if (data.NotificationChannel.DiscordId is null)
            return;

        if (
            await client.GetChannelAsync(data.NotificationChannel.DiscordId.Value)
            is not SocketTextChannel channel
        )
        {
            logger.LogWarning(
                "Discord channel {Channel} was not found",
                data.NotificationChannel.DiscordId.Value
            );
            return;
        }

        if (!botPermissionsService.HasMessagePermissions(channel.Id))
        {
            logger.LogWarning("No message permissions for {Channel}", channel.Id);
            return;
        }

        logger.LogInformation(
            "Publishing task progress ({ProgressType}) for task {TaskId} to channel {Channel}",
            progressType,
            taskId,
            channel
        );

        var locale = data.Locale.ToDiscordLocale();
        var emoji = progressType switch
        {
            ProgressType.Done => '✅',
            ProgressType.Skipped => '⏩',
            ProgressType.Undone => '❌',
            _ => '❓',
        };
        var appendage = progressType switch
        {
            ProgressType.Skipped => T("skip.appendage", locale),
            _ => string.Empty,
        };
        var body = new StringBuilder();
        body.AppendLine($"{emoji} **{data.TaskName}** {appendage}");

        if (data.PublishType is ProgressPublishType.Normal)
        {
            foreach (var task in blame.Statuses.OrderBy(t => t.Weight))
            {
                if (task.Abbreviation == data.Abbreviation)
                {
                    if (task.IsDone)
                        body.Append($"~~__{task.Abbreviation}__~~ ");
                    else
                        body.Append($"**__{task.Abbreviation}__** ");
                }
                else
                {
                    if (task.IsDone)
                        body.Append($"~~{task.Abbreviation}~~ ");
                    else
                        body.Append($"**{task.Abbreviation}** ");
                }
            }
        }
        else
        {
            foreach (var task in blame.Statuses.OrderBy(t => t.Weight))
            {
                if (task.Abbreviation == data.Abbreviation)
                {
                    if (task.IsDone)
                        body.AppendLine($"~~__{task.Name}__~~ ");
                    else
                        body.AppendLine($"**__{task.Name}__** ");
                }
                else
                {
                    if (task.IsDone)
                        body.AppendLine($"~~{task.Name}~~ ");
                    else
                        body.AppendLine($"**{task.Name}** ");
                }
            }
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(data.ProjectData, locale)
            .WithTitle(T("episode.title", locale, data.EpisodeNumber))
            .WithDescription(body.ToString())
            .Build();

        await channel.SendMessageAsync(embed: embed);
    }
}
