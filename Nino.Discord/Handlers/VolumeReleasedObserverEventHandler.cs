// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features.Queries.Observers.GetReleaseNotificationData;
using Nino.Discord.Services;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public sealed class VolumeReleasedObserverEventHandler(
    DiscordSocketClient client,
    IBotPermissionsService botPermissionsService,
    GetObserverReleaseNotificationDataHandler getDataHandler,
    ILogger<VolumeReleasedObserverEventHandler> logger
) : IEventHandler<VolumeReleasedObserverEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(VolumeReleasedObserverEvent @event)
    {
        var (_, observerId, volumeNumber, urls, commentary, publish) = @event;

        var queryResult = await getDataHandler.HandleAsync(
            new GetObserverReleaseNotificationDataQuery(observerId)
        );
        if (!queryResult.IsSuccess)
        {
            logger.LogWarning("Failed to get release data for observer {ObserverId}", observerId);
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

        if (!botPermissionsService.HasReleasePermissions(channel.Id))
        {
            logger.LogWarning("No release permissions for {Channel}", channel.Id);
            return;
        }

        var primaryRoleMention = data.PrimaryRole?.DiscordId is not null
            ? data.PrimaryRole.DiscordId.Value == channel.Guild.Id
                ? "@everyone"
                : $"<@&{data.PrimaryRole.DiscordId.Value}>"
            : string.Empty;
        var secondaryRoleMention = data.SecondaryRole?.DiscordId is not null
            ? data.SecondaryRole.DiscordId.Value == channel.Guild.Id
                ? "@everyone"
                : $"<@&{data.SecondaryRole.DiscordId.Value}>"
            : string.Empty;
        var tertiaryRoleMention = data.TertiaryRole?.DiscordId is not null
            ? data.TertiaryRole.DiscordId.Value == channel.Guild.Id
                ? "@everyone"
                : $"<@&{data.TertiaryRole.DiscordId.Value}>"
            : string.Empty;

        logger.LogInformation(
            "Publishing volume release for {ProjectTitle} {EpisodeNumber} to observer channel {Channel}",
            data.ProjectTitle,
            volumeNumber,
            channel
        );

        var locale = data.Locale.ToDiscordLocale();
        var b = new StringBuilder();

        if (!string.IsNullOrEmpty(data.ReleasePrefix))
            b.Append(data.ReleasePrefix + ' ');

        b.AppendLine(T("release.broadcast.volume", locale, data.ProjectTitle, volumeNumber));
        b.Append(
            string.Join(' ', primaryRoleMention, secondaryRoleMention, tertiaryRoleMention).Trim()
        );

        var hasCommentary = !string.IsNullOrEmpty(commentary);
        if (hasCommentary)
        {
            b.AppendLine();
            b.Append(commentary);
        }

        if (urls.Count == 1)
        {
            // If there's commentary, link will be on own line
            if (hasCommentary)
                b.AppendLine();
            else
                b.Append(' ');

            b.Append(urls[0]);
        }
        else if (urls.Count > 1)
        {
            b.AppendLine();
            b.Append(string.Join(Environment.NewLine, urls));
        }

        var message = await channel.SendMessageAsync(text: b.ToString());

        if (!publish)
            return;

        if (message.Channel.GetChannelType() == ChannelType.News) // Announcement channel
            await message.CrosspostAsync(); // Publish announcement
    }
}
