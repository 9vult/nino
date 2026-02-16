// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public class EpisodeAiredEventHandler(
    DiscordSocketClient client,
    INotificationDataService dataService,
    ILogger<EpisodeAiredEventHandler> logger
) : IEventHandler<EpisodeAiredEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(EpisodeAiredEvent @event)
    {
        var (projectId, episodeId, airTime) = @event;
        var data = await dataService.GetAirNotificationDataAsync(projectId, episodeId);

        if (data?.NotificationChannel is null)
            return;

        if (
            await client.GetChannelAsync(data.NotificationChannel.DiscordId)
            is not SocketTextChannel channel
        )
        {
            logger.LogWarning("Channel {channel} was not found", data.NotificationChannel);
            return;
        }

        var locale = data.NotificationLocale?.ToDiscordLocale() ?? channel.Guild.PreferredLocale;

        var roleMention = data.NotificationRole is not null
            ? data.NotificationRole.DiscordId == channel.Guild.Id
                ? "@everyone"
                : $"<@&{data.NotificationRole.DiscordId}>"
            : string.Empty;
        var userMention = data.NotificationUser is not null
            ? $"<@{data.NotificationUser.DiscordId}>"
            : string.Empty;
        var header = $"{data.ProjectTitle} ({data.ProjectType.ToFriendlyString(locale)})";
        var absoluteTime = $"<t:{airTime.ToUnixTimeSeconds()}:D>";
        var relativeTime = $"<t:{airTime.ToUnixTimeSeconds()}:R>";

        var embed = new EmbedBuilder()
            .WithAuthor(header, data.AniListUrl)
            .WithTitle(T("episode.aired.title", locale, data.EpisodeNumber))
            .WithDescription(T("episode.aired.body", locale, absoluteTime, relativeTime))
            .WithThumbnailUrl(data.PosterUrl)
            .WithCurrentTimestamp()
            .Build();

        logger.LogInformation(
            "Publishing episode air notification for {ProjectTitle} {EpisodeNumber} to {Channel}",
            data.ProjectTitle,
            data.EpisodeNumber,
            channel
        );
        await channel.SendMessageAsync(embed: embed, text: $"{roleMention}{userMention}");
    }
}
