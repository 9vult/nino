// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.MarkSkipped;
using Nino.Core.Features.Commands.Tasks.MarkUndone;
using Nino.Core.Features.Queries.Episodes.GetProgressResponseData;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.Tasks.GetTaskInfo;
using Nino.Core.Features.Queries.Tasks.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Discord.Services;
using Nino.Domain;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Skip;

public class UndoneSlashCommand(
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    GetProgressResponseDataHandler getProgressResponseDataHandler,
    GetTaskInfoHandler getTaskInfoHandler,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    ResolveTaskHandler taskResolver,
    MarkTaskSkippedHandler skipHandler
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("skip", "Skip a task")]
    public async Task<RuntimeResult> HandleSkipAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        [MaxLength(Length.Number), Autocomplete(typeof(EpisodeAutocompleteHandler))] Number episode,
        [MaxLength(Length.Abbreviation), Autocomplete(typeof(EpisodeTaskAutocompleteHandler))]
            Abbreviation abbreviation
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(pId => episodeResolver.HandleAsync(new ResolveEpisodeQuery(pId, episode)))
            .ThenAsync(
                (_, eId) => taskResolver.HandleAsync(new ResolveTaskQuery(eId, abbreviation))
            );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext
                {
                    Alias = alias,
                    Episode = episode,
                    Task = abbreviation,
                }
            );
        }

        var (projectId, episodeId, taskId) = resolve.Value;

        var result = await skipHandler
            .HandleAsync(new MarkTaskSkippedCommand(projectId, episodeId, taskId, requestedBy))
            .ThenAsync(_ => getTaskInfoHandler.HandleAsync(new GetTaskInfoQuery(taskId)))
            .ThenAsync(
                (_, _) =>
                    getProgressResponseDataHandler.HandleAsync(
                        new GetProgressResponseDataQuery(episodeId)
                    )
            )
            .ThenAsync(
                (_, _, _) =>
                    getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext
                {
                    Overrides = new Dictionary<ResultStatus, string>
                    {
                        [ResultStatus.BadRequest] = "undone.badRequest",
                    },
                }
            );
        }

        var episodeIsDone = result.Value.Item1;
        var taskInfo = result.Value.Item2;
        var progressResponseInfo = result.Value.Item3;
        var pData = result.Value.Item4;

        var body = new StringBuilder();
        body.AppendLine(T("skip.success", locale, taskInfo.EpisodeNumber, taskInfo.TaskName));

        if (episodeIsDone)
            body.AppendLine(T("done.episodeComplete", locale, taskInfo.EpisodeNumber));

        if (progressResponseInfo.ProgressResponseType is ProgressResponseType.Verbose)
        {
            body.AppendLine();
            foreach (var task in progressResponseInfo.Statuses.OrderBy(t => t.Weight))
            {
                if (task.IsDone)
                    body.Append($"~~{task.Abbreviation}~~ ");
                else
                    body.Append($"**{task.Abbreviation}** ");
            }
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle($"⏩ {T("skip.response.title", locale)}")
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
