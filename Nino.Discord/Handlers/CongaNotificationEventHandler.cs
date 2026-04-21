// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features.Queries.Tasks.GetCongaNotificationData;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public sealed class CongaNotificationEventHandler(
    DiscordSocketClient client,
    GetCongaNotificationDataHandler getCongaNotifHandler,
    ILogger<CongaNotificationEventHandler> logger
) : IEventHandler<CongaNotificationEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(CongaNotificationEvent @event)
    {
        var (_, episodeId, readOnlyList, isReminder) = @event;

        var queryResult = await getCongaNotifHandler.HandleAsync(
            new GetCongaNotificationDataQuery(episodeId, readOnlyList)
        );
        if (!queryResult.IsSuccess)
            return;

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

        var locale = data.Locale.ToDiscordLocale();
        var localizationKey = isReminder ? "conga.notification.reminder" : "conga.notification";

        List<Task> msgTasks = [];
        foreach (var chunk in data.Staff.Chunk(13))
        {
            var content = new StringBuilder();
            foreach (var pingee in data.Staff)
            {
                var userMention = $"<@{pingee.Assignee.DiscordId}>";
                content.Append(
                    data.PrefixType switch
                    {
                        CongaPrefixType.Nickname => $"[{data.ProjectNickname}] ",
                        CongaPrefixType.Title => $"[{data.ProjectData.ProjectTitle}] ",
                        _ => string.Empty,
                    }
                );

                content.Append(
                    T(localizationKey, locale, userMention, data.EpisodeNumber, pingee.TaskName)
                );

                logger.LogInformation(
                    "Publishing conga notification for {ProjectTitle} {EpisodeNumber}'s {Task} to {Channel}",
                    data.ProjectData.ProjectTitle,
                    data.EpisodeNumber,
                    pingee.TaskName,
                    channel
                );
            }
            msgTasks.Add(channel.SendMessageAsync(content.ToString()));
        }

        await Task.WhenAll(msgTasks);
    }
}
