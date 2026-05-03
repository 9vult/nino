// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features.Queries.Episodes.GetAirNotificationData;
using Nino.Discord.Interactions;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public sealed class EpisodeAiredEventHandler(
    DiscordSocketClient client,
    GetAirNotificationDataHandler getDataHandler,
    ILogger<EpisodeAiredEventHandler> logger
) : IEventHandler<EpisodeAiredEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(EpisodeAiredEvent @event)
    {
        var (_, episodeId, airTime) = @event;
        var queryResult = await getDataHandler.HandleAsync(
            new GetAirNotificationDataQuery(episodeId)
        );
        if (!queryResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to get air notification data for episode {EpisodeId}",
                episodeId
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

        var roleMention = data.NotificationRole?.DiscordId is not null
            ? data.NotificationRole.DiscordId.Value == channel.Guild.Id
                ? "@everyone"
                : $"<@&{data.NotificationRole.DiscordId.Value}>"
            : string.Empty;
        var userMention = data.NotificationUser?.DiscordId is not null
            ? $"<@{data.NotificationUser.DiscordId.Value}>"
            : string.Empty;

        var locale = data.Locale.ToDiscordLocale();
        var absoluteTime = $"<t:{airTime.ToUnixTimeSeconds()}:D>";
        var relativeTime = $"<t:{airTime.ToUnixTimeSeconds()}:R>";

        var embed = new EmbedBuilder()
            .WithProjectInfo(data.ProjectData, locale)
            .WithTitle(T("episode.aired.title", locale, data.EpisodeNumber))
            .WithDescription(T("episode.aired.body", locale, absoluteTime, relativeTime))
            .Build();

        logger.LogInformation(
            "Publishing episode air notification for {ProjectTitle} {EpisodeNumber} to {Channel}",
            data.ProjectData.ProjectTitle,
            data.EpisodeNumber,
            channel
        );
        await channel.SendMessageAsync(embed: embed, text: $"{roleMention}{userMention}");
    }
}
