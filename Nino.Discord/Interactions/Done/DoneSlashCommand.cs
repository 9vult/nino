// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Nino.Core.Dtos;
using Nino.Core.Enums;
using Nino.Core.Features.Done;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    public async Task<RuntimeResult> HandleDoneAsync(
        [MaxLength(32)] string alias,
        [MaxLength(16)] string abbreviation,
        [MaxLength(32)] string? episodeNumber = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        episodeNumber = episodeNumber?.Trim();

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        string taskName;
        DoneStateDto commandDto;
        if (episodeNumber is not null)
        {
            taskName = await dataService.GetTaskNameAsync(projectId, episodeNumber, abbreviation);
            commandDto = new DoneStateDto(
                projectId,
                episodeNumber,
                episodeNumber,
                abbreviation,
                taskName,
                requestedBy
            );
            // Check if the episode has aired
            if (await dataService.GetHasEpisodeAiredAsync(projectId, episodeNumber))
                return await HandleFinalAsync(interaction, commandDto);
            // The episode hasn't aired, get confirmation
            return await SendUnairedEmbedAsync(interaction, commandDto);
        }

        // We need to look for an episode
        var workingEpisode = await dataService.GetWorkingEpisodeAsync(projectId);
        if (workingEpisode is null)
            return await interaction.FailAsync(T("episode.allComplete", locale));

        var taskEpisode = await dataService.GetWorkingTaskEpisodeAsync(projectId, abbreviation);
        if (taskEpisode is null)
        {
            // Does the task even exist?
            var taskExists = await dataService.GetDoesTaskExistAsync(projectId, abbreviation);
            return await interaction.FailAsync(
                taskExists
                    ? T("task.complete.alreadyDone", locale, abbreviation)
                    : T("error.taskNotFound", locale, abbreviation)
            );
        }

        taskName = await dataService.GetTaskNameAsync(projectId, taskEpisode, abbreviation);
        commandDto = new DoneStateDto(
            projectId,
            workingEpisode,
            taskEpisode,
            abbreviation,
            taskName,
            requestedBy
        );
        // Check if the next episode awaiting the task is the working episode
        if (taskEpisode == workingEpisode)
            return await HandleFinalAsync(interaction, commandDto);
        // It's not the working episode, get confirmation
        return await SendAheadEmbedAsync(interaction, commandDto, workingEpisode);
    }
}
