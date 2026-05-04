// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.Tasks.GetTaskInfo;
using Nino.Core.Features.Queries.Tasks.GetWorkingTaskEpisode;
using Nino.Core.Features.Queries.Tasks.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    [SlashCommand("done", "Mark a task done")]
    public async Task<RuntimeResult> HandleDoneAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(ProjectTaskAutocompleteHandler))]
            Abbreviation abbreviation,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))]
            Number? episodeNumber = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        ProjectId projectId;
        EpisodeId episodeId;
        TaskId taskId;

        GetGenericProjectDataResponse pData;
        GetTaskInfoResponse tData;

        if (episodeNumber.HasValue)
        {
            var resolve = await projectResolver
                .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
                .ThenAsync(pId =>
                    episodeResolver.HandleAsync(new ResolveEpisodeQuery(pId, episodeNumber.Value))
                )
                .ThenAsync(
                    (_, eId) => taskResolver.HandleAsync(new ResolveTaskQuery(eId, abbreviation))
                )
                .ThenAsync((_, _, tId) => getTaskInfoHandler.HandleAsync(new GetTaskInfoQuery(tId)))
                .ThenAsync(
                    (pId, _, _, _) =>
                        getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(pId))
                );

            if (!resolve.IsSuccess)
            {
                return await interaction.FailAsync(
                    resolve.Status,
                    locale,
                    new FailureContext
                    {
                        Alias = alias,
                        Episode = episodeNumber,
                        Task = abbreviation,
                    }
                );
            }

            (projectId, episodeId, taskId, tData, pData) = resolve.Value;
        }
        else // Episode not specified
        {
            var resolve = await projectResolver
                .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
                .ThenAsync(pId =>
                    getWorkingTaskEpisodeHandler.HandleAsync(
                        new GetWorkingTaskEpisodeQuery(pId, abbreviation)
                    )
                )
                .ThenAsync(
                    (_, wte) => getTaskInfoHandler.HandleAsync(new GetTaskInfoQuery(wte.TaskId))
                )
                .ThenAsync(
                    (pId, _, _) =>
                        getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(pId))
                );

            if (!resolve.IsSuccess)
            {
                switch (resolve.Status)
                {
                    case ResultStatus.EpisodeNotFound:
                        return await interaction.FailAsync(T("done.noIncompleteEpisodes", locale));
                    case ResultStatus.TaskNotFound when resolve.Message == "all-complete":
                        return await interaction.FailAsync(T("done.noIncompleteTasks", locale));
                    case ResultStatus.TaskNotFound:
                        return await interaction.FailAsync(
                            T("task.resolutionFailed", locale, abbreviation)
                        );
                    default:
                        return await interaction.FailAsync(
                            resolve.Status,
                            locale,
                            new FailureContext { Alias = alias, Task = abbreviation }
                        );
                }
            }

            projectId = resolve.Value.Item1;
            episodeId = resolve.Value.Item2.TaskEpisodeId;
            taskId = resolve.Value.Item2.TaskId;
            tData = resolve.Value.Item3;
            pData = resolve.Value.Item4;

            // Working episode and task episode differ, request user confirmation
            if (resolve.Value.Item2.Difference > 0)
                return await SendAheadEmbedAsync(
                    interaction,
                    requestedBy,
                    pData,
                    tData,
                    resolve.Value.Item2
                );
        }

        // Perform episode aired check
        if (tData.EpisodeNumber.IsDecimal(out var decimalNumber))
        {
            var airCheckResult = await aniListService.GetEpisodeAirTimeAsync(
                pData.AniListId,
                decimalNumber
            );
            if (airCheckResult.IsSuccess && airCheckResult.Value > DateTimeOffset.UtcNow)
            {
                return await SendUnairedEmbedAsync(interaction, requestedBy, pData, tData);
            }
        }

        // All good, onward!
        return await HandleFinalAsync(
            interaction,
            projectId,
            episodeId,
            taskId,
            requestedBy,
            pData
        );
    }
}
