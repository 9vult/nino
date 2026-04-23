// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features.Queries.Episodes.GetAirNotificationData;
using Nino.Core.Features.Queries.Projects.GetReleaseNotificationData;
using Nino.Discord.Interactions;
using Nino.Discord.Services;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public sealed class EpisodeReleasedEventHandler(
    DiscordSocketClient client,
    IBotPermissionsService botPermissionsService,
    GetReleaseNotificationDataHandler getDataHandler,
    ILogger<EpisodeReleasedEventHandler> logger
) : IEventHandler<EpisodeReleasedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(EpisodeReleasedEvent @event)
    {
        var (
            projectId,
            episodeNumber,
            urls,
            publish,
            primaryRoleId,
            secondaryRoleId,
            tertiaryRoleId
        ) = @event;

        var queryResult = await getDataHandler.HandleAsync(
            new GetReleaseNotificationDataQuery(
                projectId,
                primaryRoleId,
                secondaryRoleId,
                tertiaryRoleId
            )
        );
        if (!queryResult.IsSuccess)
        {
            logger.LogWarning("Failed to get release data for project {ProjectId}", projectId);
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
            "Publishing episode release for {ProjectTitle} {EpisodeNumber} to {Channel}",
            data.ProjectTitle,
            episodeNumber,
            channel
        );

        var locale = data.Locale.ToDiscordLocale();
        var b = new StringBuilder();

        b.AppendLine(T("release.broadcast.episode", locale, data.ProjectTitle, episodeNumber));
        b.Append(string.Join(' ', primaryRoleMention, secondaryRoleMention, tertiaryRoleMention));

        if (urls.Count == 1)
        {
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
