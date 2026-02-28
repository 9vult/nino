// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Events;
using Nino.Core.Features.Episodes.Roster;
using Nino.Core.Services;
using Nino.Localization;

namespace Nino.Discord.Handlers;

public class TaskCompletedEventHandler(
    DiscordSocketClient client,
    IDataService dataService,
    EpisodeRosterHandler rosterHandler,
    ILogger<TaskCompletedEventHandler> logger
) : IEventHandler<TaskCompletedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TaskCompletedEvent @event)
    {
        var (projectId, episodeId, abbreviation, wasSkipped, _) = @event;
        var progData = await dataService.GetTaskProgressDataAsync(
            projectId,
            episodeId,
            abbreviation
        );
        var locale = progData.Locale.ToDiscordLocale();

        var channelId = progData.UpdateChannel.DiscordId;
        if (channelId is null)
        {
            logger.LogWarning("Project {ProjectId} has no update channel", projectId);
            return;
        }
        if (await client.GetChannelAsync(channelId.Value) is not SocketTextChannel updateChannel)
        {
            logger.LogWarning(
                "Failed to find channel {ChannelId} for project {ProjectId}",
                channelId,
                projectId
            );
            return;
        }

        var basicData = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{basicData.Title} ({basicData.Type.ToFriendlyString(locale)})";

        // TODO: omit for single-episode movies
        var title = T("episode.title", locale, progData.EpisodeNumber);

        var body = new StringBuilder();
        body.AppendLine($"{(wasSkipped ? ":fast_forward:" : "✅")} **{progData.FullName}**");

        var rosterResult = await rosterHandler.HandleAsync(
            new EpisodeRosterCommand(projectId, progData.EpisodeNumber, basicData.Owner.Id!.Value)
        );

        List<string> statuses = [];
        foreach (var task in rosterResult.Value!.Tasks.OrderBy(t => t.Weight))
        {
            statuses.Add(task.IsDone ? $"~~{task.Abbreviation}~~" : $"**{task.Abbreviation}**");
        }
        body.AppendLine(); // Blank line
        body.AppendLine(string.Join(" ", statuses));

        logger.LogInformation(
            "Publishing progress embed for project {ProjectId} to channel {ChannelId}",
            projectId,
            channelId
        );

        var embed = new EmbedBuilder()
            .WithAuthor(header, url: basicData.AniListUrl)
            .WithTitle(title)
            .WithDescription(body.ToString())
            .WithThumbnailUrl(basicData.PosterUrl)
            .WithCurrentTimestamp()
            .Build();

        try
        {
            await updateChannel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send update message to channel {ChannelId}", channelId);
        }
    }
}
