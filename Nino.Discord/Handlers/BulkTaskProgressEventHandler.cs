// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features.Queries.Tasks.GetBulkProgressNotificationData;
using Nino.Discord.Interactions;
using Nino.Discord.Services;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public sealed class BulkTaskProgressEventHandler(
    DiscordSocketClient client,
    IBotPermissionsService botPermissionsService,
    GetBulkProgressNotificationDataHandler getDataHandler,
    ILogger<BulkTaskProgressEventHandler> logger
) : IEventHandler<BulkTaskProgressEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(BulkTaskProgressEvent @event)
    {
        var (projectId, firstEpisodeId, lastEpisodeId, abbreviation, progressType) = @event;

        var queryResult = await getDataHandler.HandleAsync(
            new GetBulkProgressNotificationDataQuery(
                projectId,
                firstEpisodeId,
                lastEpisodeId,
                abbreviation
            )
        );
        if (!queryResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to get bulk progress data for task {Abbreviation}",
                abbreviation
            );
            return;
        }

        var data = queryResult.Value;

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
            "Publishing bulk task progress ({ProgressType}) for project {ProjectId} task {abbreviation} episodes {First}-{Last} to channel {Channel}",
            progressType,
            projectId,
            abbreviation,
            data.FirstEpisodeNumber,
            data.LastEpisodeNumber,
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

        var embed = new EmbedBuilder()
            .WithProjectInfo(data.ProjectData, locale)
            .WithTitle(
                T("bulk.publish.title", locale, data.FirstEpisodeNumber, data.LastEpisodeNumber)
            )
            .WithDescription(body.ToString())
            .Build();

        await channel.SendMessageAsync(embed: embed);
    }
}
