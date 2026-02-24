// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Dtos;
using Nino.Core.Enums;
using Nino.Core.Features.Done;
using Nino.Core.Features.Episodes.Roster;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    private async Task<RuntimeResult> HandleFinalAsync(
        SocketInteraction interaction,
        DoneStateDto state
    )
    {
        var (projectId, _, taskEpisodeNumber, abbreviation, taskName, requestedBy) = state;
        logger.LogInformation(
            "Handling specified /done by user {RequestedBy} for project {ProjectId} episode {EpisodeNumber}",
            requestedBy,
            projectId,
            taskEpisodeNumber
        );

        var locale = interaction.UserLocale;
        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var doneResult = await doneHandler.HandleAsync(
            new DoneCommand(projectId, taskEpisodeNumber, abbreviation, requestedBy)
        );

        if (doneResult.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                doneResult.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.taskNotFound", abbreviation),
                    ResultStatus.BadRequest => T(
                        "keyStaff.creation.conflict",
                        locale,
                        abbreviation
                    ),
                    _ => T("error.generic", locale),
                }
            );
        }

        var body = new StringBuilder();
        body.AppendLine(T("task.complete.success", locale, taskName, taskEpisodeNumber));

        // Episode is complete
        if (doneResult.Value)
            body.AppendLine(T("task.complete.episodeDone", locale, taskEpisodeNumber));

        // Generate status line if needed by display settings
        var progressData = await dataService.GetTaskProgressDataAsync(
            projectId,
            taskEpisodeNumber,
            abbreviation
        );
        if (progressData.ProgressResponseType == ProgressResponseType.Verbose)
        {
            var rosterDto = new EpisodeRosterCommand(projectId, taskEpisodeNumber, requestedBy);
            var rosterResult = await rosterHandler.HandleAsync(rosterDto);

            List<string> statuses = [];
            foreach (var task in rosterResult.Value!.Tasks.OrderBy(t => t.Weight))
            {
                statuses.Add(task.IsDone ? $"~~{task.Abbreviation}~~" : $"**{task.Abbreviation}**");
            }

            body.AppendLine(); // Blank line
            body.AppendLine(string.Join(" ", statuses));
        }

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("task.complete.title", locale))
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
